namespace Messenger.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Messenger.Api.Data;
using Messenger.Api.Models;

[ApiController]
[Route("messages")]
public class MessagesController : ControllerBase
{
    private readonly MessengerContext _context;

    public MessagesController(MessengerContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest("Message text cannot be empty.");

        var senderExists = await _context.Users.AnyAsync(u => u.Id == request.SenderId);
        if (!senderExists)
            return NotFound("Sender user does not exist.");

        var convExists = await _context.Conversations.AnyAsync(c => c.Id == request.ConversationId);
        if (!convExists)
            return NotFound("Conversation does not exist. Please create one first.");

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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(Guid id)
    {
        var message = await _context.Messages.FindAsync(id);

        if (message == null)
            return NotFound("Message not found.");

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public record SendMessageRequest(Guid ConversationId, Guid SenderId, string Text);