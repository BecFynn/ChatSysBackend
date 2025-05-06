using ChatSysBackend.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatSysBackend.Controllers;

public class DataContext : DbContext
{
    
    public DbSet<User> Users { get; set; }
    public DbSet<Groupchat> Groupchats { get; set; }
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
        
    }

    public DataContext()
    {
        
    }

}