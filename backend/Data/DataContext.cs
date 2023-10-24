using snake_game.Models;

namespace snake_game.Data;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Tournament> Tournaments { get; set; }
    public DbSet<Participation> Participations { get; set; }
    public DbSet<Round> Rounds { get; set; }
    public DbSet<Map> Maps { get; set; }
    public DbSet<MapObstacle> MapObstacles { get; set; }
    public DbSet<Obstacle> Obstacles { get; set; }
    public DbSet<Point> Points { get; set; }

    public DataContext(DbContextOptions<DataContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}
