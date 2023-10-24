using snake_game.Models;

public class DbInitializer
{
    private readonly DataContext _context;

    public DbInitializer(DataContext context)
    {
        _context = context;

        SeedUsersIfEmpty();
        SeedObstaclesIfEmpty();
    }

    private void SeedUsersIfEmpty()
    {
        if (_context.Users.Any())
            return;

        var initialUsers = new[]
        {
            new User
            {
                Username = "naudotojas1",
                Email = "email1@email.com",
                PasswordHash = "passHash",
                UserType = UserType.Basic
            },
            new User
            {
                Username = "naudotojas2",
                Email = "email2@email.com",
                PasswordHash = "passHash",
                UserType = UserType.Basic
            },
            new User
            {
                Username = "admin1",
                Email = "email3@email.com",
                PasswordHash = "passHash",
                UserType = UserType.Admin
            },
        };

        _context.Users.AddRange(initialUsers);
        _context.SaveChanges();
    }

    private void SeedObstaclesIfEmpty()
    {
        if (_context.Obstacles.Any())
            return;

        var initialObstacles = new[]
        {
            new Obstacle
            {
                Points = new List<Point>
                {
                    new Point { X = 0, Y = 0 },
                    new Point { X = 1, Y = 0 }
                },
            },
            new Obstacle
            {
                Points = new List<Point>
                {
                    new Point { X = -1, Y = -1 },
                    new Point { X = 0, Y = 0 },
                    new Point { X = 1, Y = 1 }
                },
            },
            new Obstacle
            {
                Points = new List<Point>
                {
                    new Point { X = 0, Y = -1 },
                    new Point { X = -1, Y = 0 },
                    new Point { X = 0, Y = 0 },
                    new Point { X = 1, Y = 0 },
                    new Point { X = 0, Y = 1 }
                }
            }
        };

        _context.Obstacles.AddRange(initialObstacles);
        _context.SaveChanges();
    }
}
