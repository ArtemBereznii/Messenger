namespace Messenger.Api.Models;

public class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = "direct"; // (direct/group)
}