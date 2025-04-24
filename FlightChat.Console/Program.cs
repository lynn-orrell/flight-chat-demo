using System.Diagnostics;
using Azure.Identity;
using dotenv.net;
using FlightChat;
using FlightChat.GroupChat;
using Microsoft.SemanticKernel;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

DotEnv.Fluent().WithProbeForEnv().Load();

string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
    ?? throw new InvalidOperationException("Environment variable 'AZURE_OPENAI_ENDPOINT' is not set.");

string deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
    ?? throw new InvalidOperationException("Environment variable 'AZURE_OPENAI_DEPLOYMENT_NAME' is not set.");

Uri otelEndpoint = new Uri(Environment.GetEnvironmentVariable("OTEL_ENDPOINT")
    ?? throw new InvalidOperationException("Environment variable 'OTEL_ENDPOINT' is not set."));

bool logHttpRequests = bool.Parse(Environment.GetEnvironmentVariable("LOG_HTTP_REQUESTS") ?? "false");

Directory.CreateDirectory("./charts");

AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

var builder = Host.CreateApplicationBuilder(args);

ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault().AddService("FlightChat");

var traceProviderBuilder = Sdk.CreateTracerProviderBuilder()
                              .SetResourceBuilder(resourceBuilder)
                              .AddSource("FlightChat")
                              .AddSource("Microsoft.SemanticKernel*")
                              .AddOtlpExporter(options => options.Endpoint = otelEndpoint);

using var traceProvider = traceProviderBuilder.Build();

using var meterProvider = Sdk.CreateMeterProviderBuilder()
                             .SetResourceBuilder(resourceBuilder)
                             .AddMeter("Microsoft.SemanticKernel*")
                             .AddOtlpExporter(options => options.Endpoint = otelEndpoint)
                             .Build();

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(resourceBuilder);
        options.AddOtlpExporter(options => options.Endpoint = otelEndpoint);
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
    });
    builder.SetMinimumLevel(LogLevel.Debug);
});

ActivitySource flightChatActivitySource = new("FlightChat");

builder.Services.AddSingleton(traceProvider);
builder.Services.AddSingleton(meterProvider);
builder.Services.AddSingleton(loggerFactory);
builder.Services.AddSingleton(flightChatActivitySource);
builder.Services.AddHttpClient();
builder.Services.AddHostedService<Worker>();
builder.Services.AddTransient<FlightGroupChat>();

builder.Services.AddAzureOpenAIChatCompletion(deployment, endpoint, new AzureCliCredential());
builder.Services.AddKernel();

var host = builder.Build();
host.Run();
