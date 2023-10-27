using System.Globalization;
using snake_game.Models;

public class DbInitializer
{
    private readonly DataContext _context;

    public DbInitializer(DataContext context)
    {
        _context = context;

        SeedObstaclesIfEmpty();
        SeedUsersIfEmpty();
        SeedMapsIfEmpty();
        SeedTournamentsIfEmpty();
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

    private void SeedMapsIfEmpty()
    {
        if (_context.Maps.Any())
            return;

        var users = _context.Users.ToList();

        var initialMaps = new[]
        {
            new Map
            {
                Name = "naudotojo1_zemelapis1",
                IsPublished = true,
                Rating = (double)(5 + 4) / 2,
                Creator = users[0],
                MapObstacles = new List<MapObstacle>
                {
                    new MapObstacle
                    {
                        ObstacleId = 1,
                        Position = new Point { X = 1, Y = 1 }
                    },
                    new MapObstacle
                    {
                        ObstacleId = 2,
                        Position = new Point { X = 1, Y = 3 }
                    },
                    new MapObstacle
                    {
                        ObstacleId = 3,
                        Position = new Point { X = 5, Y = 4 }
                    }
                }
            },
            new Map
            {
                Name = "naudotojo2_zemelapis1",
                IsPublished = true,
                Rating = (double)(5 + 2) / 2,
                Creator = users[1],
                MapObstacles = new List<MapObstacle>
                {
                    new MapObstacle
                    {
                        ObstacleId = 1,
                        Position = new Point { X = 4, Y = 4 }
                    },
                    new MapObstacle
                    {
                        ObstacleId = 2,
                        Position = new Point { X = 2, Y = 5 }
                    },
                    new MapObstacle
                    {
                        ObstacleId = 3,
                        Position = new Point { X = 1, Y = 4 }
                    }
                }
            },
            new Map
            {
                Name = "naudotojo2_zemelapis2",
                IsPublished = true,
                Rating = 0,
                Creator = users[1],
                MapObstacles = new List<MapObstacle>
                {
                    new MapObstacle
                    {
                        ObstacleId = 1,
                        Position = new Point { X = 1, Y = 1 }
                    },
                    new MapObstacle
                    {
                        ObstacleId = 2,
                        Position = new Point { X = 2, Y = 2 }
                    },
                    new MapObstacle
                    {
                        ObstacleId = 3,
                        Position = new Point { X = 3, Y = 3 }
                    }
                }
            }
        };

        var MapRatingsForMap1 = new List<MapRating>
        {
            new MapRating
            {
                Map = initialMaps[0],
                User = users[1],
                Rating = 5
            },
            new MapRating
            {
                Map = initialMaps[0],
                User = users[2],
                Rating = 4
            },
        };
        initialMaps[0].MapRatings = MapRatingsForMap1;

        var MapRatingsForMap2 = new List<MapRating>
        {
            new MapRating
            {
                Map = initialMaps[1],
                User = users[2],
                Rating = 5
            },
            new MapRating
            {
                Map = initialMaps[1],
                User = users[0],
                Rating = 2
            },
        };
        initialMaps[1].MapRatings = MapRatingsForMap2;

        _context.Maps.AddRange(initialMaps);
        _context.SaveChanges();
    }

    private void SeedTournamentsIfEmpty()
    {
        if (_context.Tournaments.Any())
            return;

        var users = _context.Users.ToList();
        var maps = _context.Maps.ToList();

        var initialTournaments = new[]
        {
            new Tournament
            {
                Name = "naudotojo3_turnyras1",
                StartTime = DateTime.ParseExact(
                    "2024-05-01T08:30:52Z",
                    "yyyy-MM-ddTHH:mm:ssZ",
                    System.Globalization.CultureInfo.InvariantCulture
                ).ToUniversalTime(),
                EndTime = DateTime.ParseExact(
                    "2024-05-01T09:30:52Z",
                    "yyyy-MM-ddTHH:mm:ssZ",
                    System.Globalization.CultureInfo.InvariantCulture
                ).ToUniversalTime(),
                CurrentRound = 1,
                MaxParticipants = 2,
                Organizer = users[2],
            },
            new Tournament
            {
                Name = "naudotojo3_turnyras2",
                StartTime = DateTime.ParseExact(
                    "2024-05-01T10:30:52Z",
                    "yyyy-MM-ddTHH:mm:ssZ",
                    System.Globalization.CultureInfo.InvariantCulture
                ).ToUniversalTime(),
                EndTime = DateTime.ParseExact(
                    "2024-05-01T11:30:52Z",
                    "yyyy-MM-ddTHH:mm:ssZ",
                    System.Globalization.CultureInfo.InvariantCulture
                ).ToUniversalTime(),
                CurrentRound = 1,
                MaxParticipants = 2,
                Organizer = users[2],
            }
        };

        var RoundsForTournament1 = new List<Round>
        {
            new Round
            {
                Index = 1,
                Map = maps[0],
                Tournament = initialTournaments[0],
            },
            new Round
            {
                Index = 2,
                Map = maps[1],
                Tournament = initialTournaments[0],
            },
        };
        initialTournaments[0].Rounds = RoundsForTournament1;

        var RoundsForTournament2 = new List<Round>
        {
            new Round
            {
                Index = 1,
                Map = maps[1],
                Tournament = initialTournaments[1],
            },
            new Round
            {
                Index = 2,
                Map = maps[0],
                Tournament = initialTournaments[1],
            },
        };
        initialTournaments[1].Rounds = RoundsForTournament2;

        var ParticipationsForTournament2 = new List<Participation>
        {
            new Participation { User = users[0], Tournament = initialTournaments[1] },
            new Participation { User = users[1], Tournament = initialTournaments[1], }
        };
        initialTournaments[1].Participations = ParticipationsForTournament2;

        _context.Tournaments.AddRange(initialTournaments);
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
