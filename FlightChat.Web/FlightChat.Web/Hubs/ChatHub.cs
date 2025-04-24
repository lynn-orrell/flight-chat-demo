using FlightChat.Web.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using FlightChat.GroupChat;
using FlightChat.GroupChat.Models;
using FlightChat.GroupChat.PromptTemplates.Models;

namespace FlightChat.Web.Hubs;

public class ChatHub : Hub
{
    private static List<ChatMessageContent> _messages = new List<ChatMessageContent>();
    private readonly FlightGroupChat _flightGroupChat;

    public ChatHub(FlightGroupChat flightGroupChat)
    {
        _flightGroupChat = flightGroupChat;

        _flightGroupChat.GroupChatResponseGenerated += OnGroupChatResponseGenerated;
        _flightGroupChat.GroupChatSelectionStrategyResult += OnGroupChatSelectionStrategyResult;
        _flightGroupChat.GroupChatTerminationStrategyResult += OnGroupChatTerminationStrategyResult;
    }

    public async Task SendMessage(string message)
    {
        _messages.Add(new ChatMessageContent(AuthorRole.User, message));

        await _flightGroupChat.AddChatMessagesAsync(_messages);

        await _flightGroupChat.StartGroupChat();
    }

    private void OnGroupChatResponseGenerated(object? sender, GroupChatResponseGeneratedEventArgs e)
    {
        _messages.Add(e.ChatMessageContent);
        Clients.All.SendAsync("ReceiveMessage", new ConversationNotification(NotificationType.AssistantMessage, Guid.NewGuid().ToString(), e.ChatMessageContent.ToString())).Wait();
    }

    private void OnGroupChatSelectionStrategyResult(object? sender, AgentSelectionStrategyResponse e)
    {
        Clients.All.SendAsync("SpeakerChange", new ConversationNotification(NotificationType.SpeakerChange, Guid.NewGuid().ToString(), e.NextAgent)).Wait();
    }

    private void OnGroupChatTerminationStrategyResult(object? sender, AgentTerminationStrategyResponse e)
    {
        Clients.All.SendAsync("AgentGroupChatTerminationUpdate", new ConversationNotification(NotificationType.AgentGroupChatTerminationUpdate, Guid.NewGuid().ToString(), e.TerminationReason)).Wait();

        if(e.ShouldTerminate)
        {
            Clients.All.SendAsync("AgentGroupChatComplete", new ConversationNotification(NotificationType.AgentGroupChatComplete, Guid.NewGuid().ToString(), string.Empty)).Wait();
        }
    }
}