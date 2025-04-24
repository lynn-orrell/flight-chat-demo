using FlightChat.GroupChat.PromptTemplates.Models;

namespace FlightChat.GroupChat.Models
{
    public class GroupChatTerminationStrategyResponseEventArgs : EventArgs
    {
        public AgentTerminationStrategyResponse AgentTerminationStrategyResponse { get; }

        public GroupChatTerminationStrategyResponseEventArgs(AgentTerminationStrategyResponse agentTerminationStrategyResponse)
        {
            AgentTerminationStrategyResponse = agentTerminationStrategyResponse;
        }
    }
}