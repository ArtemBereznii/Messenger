using Microsoft.EntityFrameworkCore;
using Messenger.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework to use SQLite
builder.Services.AddDbContext<MessengerContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();