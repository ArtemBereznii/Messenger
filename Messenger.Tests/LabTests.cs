namespace Messenger.Tests;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Messenger.Api.Controllers;
using Messenger.Api.Data;
using Messenger.Api.Models;
using Messenger.Api.Services;

public class UnitTests
{
    private MessengerContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MessengerContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new MessengerContext(options);
    }

    [Fact]
    public async Task TC1_1_SendMessage_ValidData_ReturnsOkAndSavesMessage()
    {
        var context = GetInMemoryDbContext();
        var controller = new MessagesController(context);

        var userId = Guid.NewGuid();
        var convId = Guid.NewGuid();
        context.Users.Add(new User { Id = userId, Name = "Test User" });
        context.Conversations.Add(new Conversation { Id = convId, Type = "direct" });
        await context.SaveChangesAsync();

        var request = new SendMessageRequest(convId, userId, "Hello Unit Test!");

        var result = await controller.SendMessage(request);

        Assert.IsType<OkObjectResult>(result);
        var savedMessage = await context.Messages.FirstOrDefaultAsync();
        Assert.NotNull(savedMessage);
        Assert.Equal("Hello Unit Test!", savedMessage.Text);
    }

    [Fact]
    public async Task TC1_2_SendMessage_EmptyText_ReturnsBadRequest()
    {
        var context = GetInMemoryDbContext();
        var controller = new MessagesController(context);

        var request = new SendMessageRequest(Guid.NewGuid(), Guid.NewGuid(), "");

        var result = await controller.SendMessage(request);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Message text cannot be empty.", badRequestResult.Value);

        var messageCount = await context.Messages.CountAsync();
        Assert.Equal(0, messageCount);
    }

    [Fact]
    public async Task TC2_1_ExportChatAsync_ValidMessages_CreatesFileAndReturnsTrue()
    {
        var exportService = new ChatExportService();
        var tempFilePath = Path.GetTempFileName();

        var messages = new List<Message>
        {
            new Message { SenderId = Guid.NewGuid(), Text = "First msg", CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
            new Message { SenderId = Guid.NewGuid(), Text = "Second msg", CreatedAt = DateTime.UtcNow }
        };

        try
        {
            var result = await exportService.ExportChatAsync(messages, tempFilePath);

            Assert.True(result);
            Assert.True(File.Exists(tempFilePath));

            var fileContent = await File.ReadAllTextAsync(tempFilePath);
            Assert.Contains("--- Chat Export ---", fileContent);
            Assert.Contains("First msg", fileContent);
            Assert.Contains("Second msg", fileContent);
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    [Fact]
    public async Task TC2_2_ExportChatAsync_EmptyMessages_ReturnsFalse()
    {
        var exportService = new ChatExportService();
        var tempFilePath = Path.GetTempFileName();
        var emptyMessages = new List<Message>(); 

        try
        {
            var result = await exportService.ExportChatAsync(emptyMessages, tempFilePath);

            Assert.False(result); 
        }
        finally
        {
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }
}