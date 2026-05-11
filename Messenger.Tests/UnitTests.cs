namespace Messenger.Tests;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Messenger.Api.Controllers;
using Messenger.Api.Data;
using Messenger.Api.Models;
using Messenger.Api.Services;

public class UnitTests
{
    // Helper: Creates a fresh, isolated database in RAM for every individual test
    private MessengerContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<MessengerContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new MessengerContext(options);
    }

    [Fact]
    public async Task CreateUser_ValidName_ReturnsCreatedAndSavesUser()
    {
        var context = GetInMemoryDbContext();
        var controller = new UsersController(context);
        var request = new CreateUserRequest("Artem Developer");

        var result = await controller.CreateUser(request);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var savedUser = Assert.IsType<User>(createdResult.Value);
        Assert.Equal("Artem Developer", savedUser.Name);
        Assert.Equal(1, await context.Users.CountAsync());
    }

    [Fact]
    public async Task CreateUser_EmptyName_ReturnsBadRequest()
    {
        var context = GetInMemoryDbContext();
        var controller = new UsersController(context);
        var request = new CreateUserRequest("   ");

        var result = await controller.CreateUser(request);

        Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(0, await context.Users.CountAsync());
    }


    [Fact]
    public async Task CreateConversation_NoType_DefaultsToDirect()
    {
        var context = GetInMemoryDbContext();
        var controller = new ConversationsController(context);
        var request = new CreateConversationRequest("");

        var result = await controller.CreateConversation(request);

        var createdResult = Assert.IsType<CreatedResult>(result);
        var conv = Assert.IsType<Conversation>(createdResult.Value);
        Assert.Equal("direct", conv.Type);
    }

    [Fact]
    public async Task GetMessages_IgnoresHiddenMessages_AndOrdersByTime()
    {
        var context = GetInMemoryDbContext();
        var controller = new ConversationsController(context);
        var convId = Guid.NewGuid();

        // Add 1 hidden message and 2 visible messages (out of order to test sorting)
        context.Messages.Add(new Message { ConversationId = convId, Text = "Visible 2", CreatedAt = DateTime.UtcNow, IsHidden = false });
        context.Messages.Add(new Message { ConversationId = convId, Text = "Hidden", CreatedAt = DateTime.UtcNow.AddMinutes(-5), IsHidden = true });
        context.Messages.Add(new Message { ConversationId = convId, Text = "Visible 1", CreatedAt = DateTime.UtcNow.AddMinutes(-10), IsHidden = false });
        await context.SaveChangesAsync();

        var result = await controller.GetMessages(convId);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var messages = Assert.IsAssignableFrom<IEnumerable<Message>>(okResult.Value).ToList();

        Assert.Equal(2, messages.Count);
        Assert.Equal("Visible 1", messages[0].Text);
        Assert.Equal("Visible 2", messages[1].Text);
    }

    [Fact]
    public async Task SendMessage_ValidRequest_ReturnsOk()
    {
        var context = GetInMemoryDbContext();
        var controller = new MessagesController(context);

        var userId = Guid.NewGuid();
        var convId = Guid.NewGuid();
        context.Users.Add(new User { Id = userId, Name = "Test" });
        context.Conversations.Add(new Conversation { Id = convId });
        await context.SaveChangesAsync();

        var request = new SendMessageRequest(convId, userId, "Test text");
        var result = await controller.SendMessage(request);

        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(1, await context.Messages.CountAsync());
    }

    [Fact]
    public async Task SendMessage_InvalidIds_ReturnsNotFound()
    {
        var context = GetInMemoryDbContext();
        var controller = new MessagesController(context);

        var request = new SendMessageRequest(Guid.NewGuid(), Guid.NewGuid(), "Test");
        var result = await controller.SendMessage(request);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteMessage_ValidId_RemovesFromDatabase()
    {
        var context = GetInMemoryDbContext();
        var controller = new MessagesController(context);

        var msgId = Guid.NewGuid();
        context.Messages.Add(new Message { Id = msgId, Text = "To Delete" });
        await context.SaveChangesAsync();

        var result = await controller.DeleteMessage(msgId);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, await context.Messages.CountAsync());
    }

    [Fact]
    public async Task ResolveReport_ActionHide_FlagsAsHidden()
    {
        var context = GetInMemoryDbContext();
        var controller = new ModerationController(context);

        var msgId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        context.Messages.Add(new Message { Id = msgId, Text = "Bad Word", IsHidden = false });
        context.Reports.Add(new Report { Id = reportId, MessageId = msgId, Reason = "Spam", IsResolved = false });
        await context.SaveChangesAsync();

        var request = new ResolveReportRequest("HIDE");
        var result = await controller.ResolveReport(reportId, request);

        Assert.IsType<OkObjectResult>(result);
        var msg = await context.Messages.FindAsync(msgId);
        Assert.NotNull(msg);
        Assert.True(msg!.IsHidden);
        Assert.Equal("Spam", msg.ModerationReason);
    }

    [Fact]
    public async Task ResolveReport_ActionDelete_RemovesRow()
    {
        var context = GetInMemoryDbContext();
        var controller = new ModerationController(context);

        var msgId = Guid.NewGuid();
        var reportId = Guid.NewGuid();
        context.Messages.Add(new Message { Id = msgId, Text = "Nuke This" });
        context.Reports.Add(new Report { Id = reportId, MessageId = msgId, IsResolved = false });
        await context.SaveChangesAsync();

        var request = new ResolveReportRequest("DELETE");

        var result = await controller.ResolveReport(reportId, request);

        var report = await context.Reports.FindAsync(reportId);
        Assert.NotNull(report);
        Assert.True(report!.IsResolved);
    }

    [Fact]
    public async Task ExportChatAsync_ValidMessages_WritesToFile()
    {
        var exportService = new ChatExportService();
        var tempFilePath = Path.GetTempFileName();

        var messages = new List<Message>
        {
            new Message { SenderId = Guid.NewGuid(), Text = "Testing export" }
        };

        try
        {
            var result = await exportService.ExportChatAsync(messages, tempFilePath);

            Assert.True(result);
            Assert.True(File.Exists(tempFilePath));
            Assert.Contains("Testing export", await File.ReadAllTextAsync(tempFilePath));
        }
        finally
        {
            if (File.Exists(tempFilePath)) File.Delete(tempFilePath);
        }
    }
}