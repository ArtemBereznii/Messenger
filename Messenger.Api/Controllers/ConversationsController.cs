namespace Messenger.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Messenger.Api.Data;
using Messenger.Api.Models;

[ApiController]
[Route("[controller]")]
public class ConversationsController : ControllerBase
{
    private readonly MessengerContext _context;

    public ConversationsController(MessengerContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var type = string.IsNullOrWhiteSpace(request.Type) ? "direct" : request.Type;

        var conversation = new Conversation
        {
            Type = type
        };

        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        return Created($"/conversations/{conversation.Id}", conversation);
    }
}

public record CreateConversationRequest(string? Type);