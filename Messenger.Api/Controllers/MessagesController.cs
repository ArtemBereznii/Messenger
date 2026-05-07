namespace Messenger.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Messenger.Api.Data;
using Messenger.Api.Models;

[ApiController]
public class MessagesController : ControllerBase
{
    private readonly MessengerContext _context;

    public MessagesController(MessengerContext context)
    {
        _context = context;
    }

    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest("Message text cannot be empty.");

        var senderExists = await _context.Users.AnyAsync(u => u.Id == request.SenderId);
        if (!senderExists)
            return NotFound("Sender user does not exist.");

        var convExists = await _context.Conversations.AnyAsync(c => c.Id == request.ConversationId);
        if (!convExists)
            return NotFound("Conversation does not exist. Please create one first."); // Returns a 404

        var message = new Message
        {
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            Text = request.Text
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return Ok(new { messageId = message.Id });
    }

    [HttpGet("conversations/{id}/messages")]
    public async Task<IActionResult> GetMessages(Guid id)
    {
        var messages = await _context.Messages
            .Where(m => m.ConversationId == id && !m.IsHidden)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return Ok(messages);
    }
}

public record SendMessageRequest(Guid ConversationId, Guid SenderId, string Text);