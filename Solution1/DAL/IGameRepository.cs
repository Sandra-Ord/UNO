using Domain;
namespace DAL;

public interface IGameRepository
{
    void SaveGame(Guid id, GameState game);
    List<(Guid id, DateTime created, DateTime updated)> GetSaveGames();
    GameState LoadGame(Guid id);
    void DeleteGame(Guid? id);
}