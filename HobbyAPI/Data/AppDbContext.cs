using HobbyAPI.Model;
using Microsoft.EntityFrameworkCore;
namespace HobbyAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public virtual DbSet<Habit> Habits { get; set; }
        public virtual DbSet<Logs> HabitsLogs { get; set; }
        public virtual DbSet<Badge> Badges { get; set; }

        // Necessário para os retornos, com enfase nos verbos http GET.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Habit>()
                .Property(h => h.goalType)
                .HasConversion<string>(); // Converte o enum para string
            modelBuilder.Entity<Logs>()
                .Property(h => h.goalType)
                .HasConversion<string>(); // Converte o enum para string
            modelBuilder.Entity<Badge>()
                .Property(h => h.badge)
                .HasConversion<string>(); // Converte o enum para string
        }
    }
}
