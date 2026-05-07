namespace Messenger.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Messenger.Api.Data;
using Messenger.Api.Models;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly MessengerContext _context;

    public UsersController(MessengerContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("User name cannot be empty.");

        var user = new User { Name = request.Name };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Created($"/users/{user.Id}", user);
    }
}
public record CreateUserRequest(string Name);