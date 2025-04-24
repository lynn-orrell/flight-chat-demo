using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using FlightChat.GroupChat;
using FlightChat.GroupChat.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

namespace FlightChat;

public class Worker : BackgroundService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly ActivitySource _activitySource;
    private readonly HttpClient _httpClient;
    private readonly FlightGroupChat _flightGroupChat;

    public Worker(FlightGroupChat flightGroupChat, IHostApplicationLifetime hostApplicationLifetime, ActivitySource activitySource, HttpClient httpClient)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _activitySource = activitySource;
        _httpClient = httpClient;
        _flightGroupChat = flightGroupChat;

        _flightGroupChat.GroupChatResponseGenerated += OnGroupChatResponseGenerated;
    }

    private void OnGroupChatResponseGenerated(object? sender, GroupChatResponseGeneratedEventArgs e)
    {
        PrettyPrint(e.ChatMessageContent, e.ChatMessageContent.Content ?? "<No Content>");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
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

            await _flightGroupChat.AddChatMessageAsync(userInput);
            await _flightGroupChat.StartGroupChat();
        }

        _hostApplicationLifetime.StopApplication();
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
}