using Microsoft.SemanticKernel;

namespace FlightChat.GroupChat.Models
{
    public class GroupChatResponseGeneratedEventArgs : EventArgs
    {
        public ChatMessageContent ChatMessageContent { get; }

        public GroupChatResponseGeneratedEventArgs(ChatMessageContent chatMessageContent)
        {
            ChatMessageContent = chatMessageContent;
        }
    }
}