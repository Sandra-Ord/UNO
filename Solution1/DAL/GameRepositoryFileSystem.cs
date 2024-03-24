using System.Runtime.Serialization;
using System.Text.Json;
using Domain;
using Helpers;
using System;
using System.IO;

namespace DAL;

public class GameRepositoryFileSystem : IGameRepository
{
    private readonly string SaveLocation = Path.Combine(Path.GetTempPath(), "Uno");
    
    public void SaveGame(Guid id, GameState game)
    {
        var content = JsonSerializer.Serialize(game, JsonHelpers.JsonSerializerOptions);
        var fileName = Path.ChangeExtension(id.ToString(), ".json");
        if (!Path.Exists(SaveLocation))
        {
            Directory.CreateDirectory(SaveLocation);
        }
        File.WriteAllText(Path.Combine(SaveLocation, fileName), content);
    }

    public List<(Guid id, DateTime created, DateTime updated)> GetSaveGames()
    {
        var games = Directory.EnumerateFiles(SaveLocation);
        var result = games
            .Select(
                path => (
                    Guid.Parse(Path.GetFileNameWithoutExtension(path)),
                    File.GetCreationTime(path),
                    File.GetLastWriteTime(path)
                )
            ).ToList();
        return result;
    }

    public GameState LoadGame(Guid id)
    {
        var fileName = Path.ChangeExtension(id.ToString(), ".json");
        var jsonStr = File.ReadAllText(Path.Combine(SaveLocation, fileName));
        var result = JsonSerializer.Deserialize<GameState>(jsonStr, JsonHelpers.JsonSerializerOptions);
        if (result == null) throw new SerializationException($"Deserialization failed for {jsonStr}");
        return result;
    }

    public void DeleteGame(Guid? id)
    {
        if (id == null) return;
        var fileName = Path.ChangeExtension(id.ToString(), ".json");
        var filePath = Path.Combine(SaveLocation, fileName!);

        if (File.Exists(filePath)) File.Delete(filePath);
    }
}