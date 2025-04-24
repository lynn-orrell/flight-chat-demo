using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using FlightChat.Plugins;
using FlightChat.PromptTemplates.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FlightChat;

public class Worker : BackgroundService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly Kernel _kernel;
    private readonly ActivitySource _activitySource;
    private readonly HttpClient _httpClient;

    public Worker(IHostApplicationLifetime hostApplicationLifetime, Kernel kernel, ActivitySource activitySource, HttpClient httpClient)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _kernel = kernel;
        _kernel.ImportPluginFromType<FlightChatPlugin>();
        _kernel.AutoFunctionInvocationFilters.Add(new TerminatingAutoFunctionInvocationFilter("ExecuteODataQuery"));
        _activitySource = activitySource;
        _httpClient = httpClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ChatCompletionAgent queryBuilderAgent = await CreateChatCompletionAgentAsync("PromptTemplates/Agents/ODataQueryBuilderAgent.yaml", _kernel, new KernelArguments()
        {
            ["odataEdm"] = await GetODataEDMAsync()
        });
        ChatCompletionAgent chartVisualizerAgent = await CreateChatCompletionAgentAsync("PromptTemplates/Agents/ChartVisualizerAgent.yaml", _kernel);
        ChatCompletionAgent markdownFormatterAgent = await CreateChatCompletionAgentAsync("PromptTemplates/Agents/MarkdownFormatterAgent.yaml", _kernel);

        KernelFunctionSelectionStrategy selectionStrategy = await CreateSelectionStrategy("PromptTemplates/Strategies/AgentSelectionStrategy.yaml", _kernel);
        KernelFunctionTerminationStrategy terminationStrategy = await CreateTerminationStrategy("PromptTemplates/Strategies/AgentTerminationStrategy.yaml", _kernel);

        AgentGroupChat chat = new(queryBuilderAgent, chartVisualizerAgent, markdownFormatterAgent)
        {
            ExecutionSettings = new()
            {
                SelectionStrategy = selectionStrategy,
                TerminationStrategy = terminationStrategy
            }
        };

        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("ASSISTANT: How can I help you? Type 'exit' to quit.");
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("USER: ");
            string? userInput = Console.ReadLine();
            Console.ResetColor();
            if (userInput == null || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            await AddChatMessageAsync(chat, userInput);
        }

        _hostApplicationLifetime.StopApplication();
    }

    private async Task AddChatMessageAsync(AgentGroupChat chat, string message)
    {
        using var activity = _activitySource.StartActivity("AddChatMessageAsync");
        activity?.AddTag("message", message);

        chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, message));
        do
        {
            await foreach (ChatMessageContent response in chat.InvokeAsync())
            {
                PrettyPrint(response, response.Content ?? "<No Content>");
            }
        } while (!chat.IsComplete);
    }

    private void PrettyPrint(ChatMessageContent chatMessageContent, string message)
    {
        Console.ForegroundColor = chatMessageContent.Role.Equals(AuthorRole.User) ? ConsoleColor.Yellow :
                                  chatMessageContent.Role.Equals(AuthorRole.Assistant) ? ConsoleColor.Gray : 
                                  ConsoleColor.White;

        Console.WriteLine($"{chatMessageContent.Role.Label.ToUpper()} [{chatMessageContent.AuthorName}]: {message}");
        Console.WriteLine();
        Console.ResetColor();
    }

    private async Task<ChatCompletionAgent> CreateChatCompletionAgentAsync(string promptTemplatePath, Kernel kernel, KernelArguments? kernelArgs = null)
    {
        var prompt = await File.ReadAllTextAsync(promptTemplatePath);
        var promptTemplateConfig = KernelFunctionYaml.ToPromptTemplateConfig(prompt);
        var agent = new ChatCompletionAgent(promptTemplateConfig, new KernelPromptTemplateFactory())
        {
            Kernel = kernel,
            Arguments = new KernelArguments(kernelArgs ?? new KernelArguments(), promptTemplateConfig.ExecutionSettings)
        };

        return agent;
    }

    private async Task<KernelFunctionSelectionStrategy> CreateSelectionStrategy(string promptTemplatePath, Kernel kernel)
    {
        KernelFunction agentSelectionFunction = KernelFunctionYaml.FromPromptYaml(await File.ReadAllTextAsync(promptTemplatePath));
        
        KernelFunctionSelectionStrategy selectionStrategy = new(agentSelectionFunction, kernel)
        {
            ResultParser = (result) =>
            {
                var selectionResponse = JsonSerializer.Deserialize<AgentSelectionStrategyResponse>(result.GetValue<string>()!);
                return selectionResponse!.NextAgent;
            },
            AgentsVariableName = "agents",
            HistoryVariableName = "history",
            // HistoryReducer = new ChatHistoryTruncationReducer(4),
        };

        return selectionStrategy;
    }

    private async Task<KernelFunctionTerminationStrategy> CreateTerminationStrategy(string promptTemplatePath, Kernel kernel, Agent[]? agentsAllowedToTerminate = null)
    {
        KernelFunction agentTerminationFunction = KernelFunctionYaml.FromPromptYaml(await File.ReadAllTextAsync(promptTemplatePath));
        
        KernelFunctionTerminationStrategy terminationStrategy = new(agentTerminationFunction, kernel)
        {
            ResultParser = (result) => 
            {
                var terminationResponse = JsonSerializer.Deserialize<AgentTerminationStrategyResponse>(result.GetValue<string>()!);
                return terminationResponse!.ShouldTerminate;
            },
            AgentVariableName = "agents",
            HistoryVariableName = "history",
            HistoryReducer = new ChatHistoryTruncationReducer(2),
            MaximumIterations = 5,
            Agents = agentsAllowedToTerminate,
            AutomaticReset = true
        };

        return terminationStrategy;
    }

    private async Task<string> GetODataEDMAsync() {
        HttpRequestMessage request = new(HttpMethod.Get, "http://localhost:5207/odata/$metadata");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        HttpResponseMessage response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return content;
    }
}