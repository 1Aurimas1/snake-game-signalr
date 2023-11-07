using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using snake_game.Models;

namespace snake_game.Data;

public class DataContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public DbSet<Tournament> Tournaments { get; set; }
    public DbSet<Participation> Participations { get; set; }
    public DbSet<Round> Rounds { get; set; }
    public DbSet<Map> Maps { get; set; }
    public DbSet<MapRating> MapRatings { get; set; }
    public DbSet<MapObstacle> MapObstacles { get; set; }
    public DbSet<Obstacle> Obstacles { get; set; }
    public DbSet<Point> Points { get; set; }
    public DbSet<Game> Games { get; set; }

    public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    //public DataContext(DbContextOptions<DataContext> options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
        //modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<IdentityUser<int>>().HasIndex(u => u.UserName).IsUnique();
        modelBuilder.Entity<IdentityUser<int>>().HasIndex(u => u.Email).IsUnique();
    }
}
