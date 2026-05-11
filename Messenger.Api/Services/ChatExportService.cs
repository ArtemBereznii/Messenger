namespace Messenger.Api.Services;

using Messenger.Api.Models;
using System.Text;

public interface IChatExportService
{
    Task<bool> ExportChatAsync(List<Message> messages, string filePath);
}

public class ChatExportService : IChatExportService
{
    public async Task<bool> ExportChatAsync(List<Message> messages, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        if (messages == null || !messages.Any())
            return false;

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("--- Chat Export ---");
            foreach (var msg in messages.OrderBy(m => m.CreatedAt))
            {
                sb.AppendLine($"[{msg.CreatedAt:yyyy-MM-dd HH:mm:ss}] User {msg.SenderId}: {msg.Text}");
            }

            await File.WriteAllTextAsync(filePath, sb.ToString());
            return true;
        }
        catch (Exception)
        {
            // In a real app we would log this, but for now we just return false
            return false;
        }
    }
}