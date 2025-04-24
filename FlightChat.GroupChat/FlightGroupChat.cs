using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using FlightChat.GroupChat.PromptTemplates.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using FlightChat.GroupChat.Plugins;
using Microsoft.Extensions.Logging;
using FlightChat.GroupChat.Models;
using System.Reflection;
using FlightChat.GroupChat.Filters;


namespace FlightChat.GroupChat
{
    public class FlightGroupChat
    {
        public event EventHandler<GroupChatResponseGeneratedEventArgs>? GroupChatResponseGenerated;
        public event EventHandler<AgentSelectionStrategyResponse>? GroupChatSelectionStrategyResult;
        public event EventHandler<AgentTerminationStrategyResponse>? GroupChatTerminationStrategyResult;
        private readonly ILogger<FlightGroupChat> _logger;
        private readonly Kernel _kernel;
        private readonly ActivitySource _activitySource;
        private AgentGroupChat? _chat;
        private readonly HttpClient _httpClient;

        public FlightGroupChat(ILogger<FlightGroupChat> logger, Kernel kernel, ActivitySource activitySource, HttpClient httpClient)
        {
            _logger = logger;
            _kernel = kernel;
            _kernel.ImportPluginFromType<FlightChatPlugin>();
            _kernel.AutoFunctionInvocationFilters.Add(new TerminatingAutoFunctionInvocationFilter("ExecuteODataQuery"));
            _activitySource = activitySource;
            _httpClient = httpClient;
        }

        public async Task AddChatMessageAsync(string message)
        {
            await Initialize();
            _chat!.AddChatMessage(new ChatMessageContent(AuthorRole.User, message));
        }

        public async Task AddChatMessagesAsync(IReadOnlyList<ChatMessageContent> messages)
        {
            await Initialize();
            _chat!.AddChatMessages(messages);
        }

        public async Task StartGroupChat()
        {
            await Initialize();

            do
            {
                await foreach (ChatMessageContent response in _chat!.InvokeAsync())
                {
                    OnGroupChatResponseGenerated(new GroupChatResponseGeneratedEventArgs(response));
                }
            } while (!_chat.IsComplete);
        }

        protected virtual void OnGroupChatResponseGenerated(GroupChatResponseGeneratedEventArgs e)
        {
            if (GroupChatResponseGenerated != null)
            {
                GroupChatResponseGenerated?.Invoke(this, e);
            }
        }

        protected virtual void OnGroupChatSelectionStrategyResult(AgentSelectionStrategyResponse e)
        {
            if (GroupChatSelectionStrategyResult != null)
            {
                GroupChatSelectionStrategyResult?.Invoke(this, e);
            }
        }

        protected virtual void OnGroupChatTerminationStrategyResult(AgentTerminationStrategyResponse e)
        {
            if (GroupChatTerminationStrategyResult != null)
            {
                GroupChatTerminationStrategyResult?.Invoke(this, e);
            }
        }

        private async Task Initialize()
        {
            if (_chat == null)
            {
                ChatCompletionAgent queryBuilderAgent = await CreateChatCompletionAgentAsync(GetPath("PromptTemplates/Agents/ODataQueryBuilderAgent.yaml"), _kernel, new KernelArguments()
                {
                    ["odataEdm"] = await GetODataEDMAsync()
                });
                ChatCompletionAgent chartVisualizerAgent = await CreateChatCompletionAgentAsync(GetPath("PromptTemplates/Agents/ChartVisualizerAgent.yaml"), _kernel);
                ChatCompletionAgent markdownFormatterAgent = await CreateChatCompletionAgentAsync(GetPath("PromptTemplates/Agents/MarkdownFormatterAgent.yaml"), _kernel);

                KernelFunctionSelectionStrategy selectionStrategy = await CreateSelectionStrategy(GetPath("PromptTemplates/Strategies/AgentSelectionStrategy.yaml"), _kernel);
                KernelFunctionTerminationStrategy terminationStrategy = await CreateTerminationStrategy(GetPath("PromptTemplates/Strategies/AgentTerminationStrategy.yaml"), _kernel);

                _chat = new(queryBuilderAgent, chartVisualizerAgent, markdownFormatterAgent)
                {
                    ExecutionSettings = new()
                    {
                        SelectionStrategy = selectionStrategy,
                        TerminationStrategy = terminationStrategy
                    }
                };
            }
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
                    OnGroupChatSelectionStrategyResult(selectionResponse!);
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
                    OnGroupChatTerminationStrategyResult(terminationResponse!);
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

        private string GetPath(string file)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, file);
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
}