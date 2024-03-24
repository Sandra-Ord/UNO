using System.Text.Json;
using Domain;
using Domain.Database;
using Helpers;

namespace DAL;

public class GameRepositoryEF : IGameRepository
{

    private readonly AppDbContext _context;

    public GameRepositoryEF(AppDbContext context)
    {
        _context = context;
    }

    public void SaveGame(Guid id, GameState state)
    {
        var game = _context.Games.FirstOrDefault(g => g.Id == state.Id);
        if (game == null)
        {
            game = new Game()
            {
                Id = state.Id,
                State = JsonSerializer.Serialize(state, JsonHelpers.JsonSerializerOptions),
                Players = state.Players.Select(p => new Domain.Database.Player()
                {
                    Id = p.Id,
                    NickName = p.NickName,
                    PlayerType = p.PlayerType
                }).ToList()
            };
            _context.Games.Add(game);
        }
        else
        {
            game.UpdatedAt = DateTime.Now;
            game.State = JsonSerializer.Serialize(state, JsonHelpers.JsonSerializerOptions);
        }

        _context.SaveChanges();
    }

    public List<(Guid id, DateTime created, DateTime updated)> GetSaveGames()
    {
        return _context
            .Games
            .OrderByDescending(g  => g.UpdatedAt)
            .ToList()
            .Select(g => (g.Id, g.CreatedAt, g.UpdatedAt))
            .ToList();
    }

    public GameState LoadGame(Guid id)
    {
        var game = _context.Games.First(g => g.Id == id);
        return JsonSerializer.Deserialize<GameState>(game.State,JsonHelpers.JsonSerializerOptions)!;
    }
    
    public void DeleteGame(Guid? id)
    {
        if (id == null) return;
        
        var game = _context.Games.Find(id);
        if (game != null)
        {
            _context.Games.Remove(game);
            _context.SaveChanges();
        }

        return;
        
    }
}