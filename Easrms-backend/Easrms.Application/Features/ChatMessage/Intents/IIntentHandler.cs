using Easrms.Application.DTOs.Chat;
using System.Threading.Tasks;

namespace Easrms.Application.Features.ChatMessage.Intents;

public interface IIntentHandler
{
    string IntentName { get; }
    Task<string> HandleAsync(ChatIntentDto intent, ChatMessageCommand command);
}
