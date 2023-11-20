using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using SnakeGame.Server.Auth.Models;
using SnakeGame.Server.Models;

namespace SnakeGame.Server.Data;

public class DataContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public DbSet<Tournament> Tournaments { get; set; } = null!;
    public DbSet<Participation> Participations { get; set; } = null!;
    public DbSet<Round> Rounds { get; set; } = null!;
    public DbSet<Map> Maps { get; set; } = null!;
    public DbSet<MapRating> MapRatings { get; set; } = null!;
    public DbSet<MapObstacle> MapObstacles { get; set; } = null!;
    public DbSet<Obstacle> Obstacles { get; set; } = null!;
    public DbSet<Point> Points { get; set; } = null!;
    public DbSet<Game> Games { get; set; } = null!;
    public DbSet<RevokedJwtToken> RevokedJwtTokens { get; set; }

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
