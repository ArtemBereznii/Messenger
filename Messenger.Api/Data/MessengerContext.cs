namespace Messenger.Api.Data;

using Microsoft.EntityFrameworkCore;
using Messenger.Api.Models;

public class MessengerContext : DbContext
{
    public MessengerContext(DbContextOptions<MessengerContext> options) : base(options) { }

    // These represent your actual database tables
    public DbSet<User> Users => Set<User>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Report> Reports => Set<Report>();
}