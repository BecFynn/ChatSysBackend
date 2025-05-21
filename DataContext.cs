using ChatSysBackend.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class DataContext : IdentityDbContext<User, UserRole, Guid>
{
    
    public DbSet<User> Users { get; set; }
    public DbSet<Groupchat> Groupchats { get; set; }
    public DbSet<Message> Messages { get; set; }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }


}