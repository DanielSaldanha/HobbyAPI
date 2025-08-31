using HobbyAPI.Model;
using Microsoft.EntityFrameworkCore;
namespace HobbyAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public virtual DbSet<Habit> Habits { get; set; }
    }
}
