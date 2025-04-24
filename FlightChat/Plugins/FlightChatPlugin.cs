using System.ComponentModel;
using System.Diagnostics;
using Microsoft.SemanticKernel;

namespace FlightChat.Plugins;

public class FlightChatPlugin
{
    private readonly ILogger _logger;
    private readonly HttpClient _client;

    public FlightChatPlugin(ILogger<FlightChatPlugin> logger, IHttpClientFactory clientFactory)
    {
        _logger = logger;
        _client = clientFactory.CreateClient();
        _client.BaseAddress = new Uri("http://localhost:5207/odata/");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    [KernelFunction]
    [Description("Executes an OData query.")]
    public async Task<string> ExecuteODataQueryAsync([Description("A valid OData query. Should follow the form: [Entity]?[Query]")]string query)
    {
        query = query.TrimStart('/');
        HttpResponseMessage response = await _client.GetAsync(query);
        var result = await response.Content.ReadAsStringAsync();
        return result;
    }

    [KernelFunction]
    [Description("Writes Apache EChart code to an HTML file.")]
    public async Task WriteEChartHtmlAsync([Description("The Apache EChart code to write to the file.")]string echartCode, [Description("The file path to write the code to.")]string filePath)
    {
        _logger.LogInformation($"Writing EChart code to file {filePath}");
        await File.WriteAllTextAsync(filePath, echartCode);
        string fullPath = Path.GetFullPath(filePath);
        Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
    }

    [KernelFunction]
    [Description("Reads Apache ECahrt code from an HTML file.")]
    public async Task<string> ReadEChartHtmlAsync([Description("The file path to read the Apache EChart code from.")]string filePath)
    {
        _logger.LogInformation($"Reading EChart code from file {filePath}");
        return await File.ReadAllTextAsync(filePath);
    }
}
