using FlightChat.GroupChat.PromptTemplates.Models;

namespace FlightChat.GroupChat.Models
{
    public class GroupChatSelectionStrategyResponseEventArgs : EventArgs
    {
        public AgentSelectionStrategyResponse AgentSelectionStrategyResponse { get; }

        public GroupChatSelectionStrategyResponseEventArgs(AgentSelectionStrategyResponse agentSelectionStrategyResponse)
        {
            AgentSelectionStrategyResponse = agentSelectionStrategyResponse;
        }
    }
}