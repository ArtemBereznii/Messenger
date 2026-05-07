namespace Messenger.Tests;

using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Messenger.Api.Models; // Ensure this matches your models namespace
using Messenger.Api.Controllers;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        // This creates a client to talk to your in-memory test server
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompleteMessengerFlow_ShouldSucceed()
    {
        // 1. Create a User
        var userResponse = await _client.PostAsJsonAsync("/users", new { Name = "Test User" });
        userResponse.EnsureSuccessStatusCode();
        var user = await userResponse.Content.ReadFromJsonAsync<User>();
        Assert.NotNull(user);

        // 2. Create a Conversation
        var convResponse = await _client.PostAsJsonAsync("/conversations", new { Type = "direct" });
        convResponse.EnsureSuccessStatusCode();
        var conversation = await convResponse.Content.ReadFromJsonAsync<Conversation>();
        Assert.NotNull(conversation);

        // 3. Send a Message
        var messageText = "Integration Test Message!";
        var msgResponse = await _client.PostAsJsonAsync("/messages/messages", new
        {
            ConversationId = conversation.Id,
            SenderId = user.Id,
            Text = messageText
        });
        msgResponse.EnsureSuccessStatusCode();

        // 4. Retrieve Message History
        var historyResponse = await _client.GetAsync($"/messages/conversations/{conversation.Id}/messages");
        historyResponse.EnsureSuccessStatusCode();
        var history = await historyResponse.Content.ReadFromJsonAsync<List<Message>>();

        // 5. Verify the message exists
        Assert.NotNull(history);
        Assert.Single(history);
        Assert.Equal(messageText, history[0].Text);
    }
}