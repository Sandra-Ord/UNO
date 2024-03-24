using ConsoleApp1;
using DAL;
using UnoEngine;
using ConsoleUI;
using Helpers;
using Microsoft.EntityFrameworkCore;

var connectionString = "DataSource=<%temppath%>uno.db;Cache=Shared";
connectionString = connectionString.Replace("<%temppath%>", Path.GetTempPath());


var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite(connectionString)
    .EnableDetailedErrors()
    .EnableSensitiveDataLogging()
    .Options;

using var db = new AppDbContext(contextOptions);
db.Database.Migrate();

IGameRepository gameRepository = new GameRepositoryEF(db);
//IGameRepository gameRepository = new GameRepositoryFileSystem();

// ----------------------------------------------------- M A I N -----------------------------------------------------
var mainMenu = ProgramMenus.GetMainMenu(NewGame, LoadGame);
// mainMenu.Run();a

// ------------------------------------------------------ E N D ------------------------------------------------------
return;

// -------------------------------------------------- N E W   G A M E ------------------------------------------------
string? NewGame()
{
    var gameEngine = new UnoGameEngine();
    PlayerSetup.ConfigurePlayers(gameEngine);
    gameEngine.SetUpGame();
    var gameController = new GameController(gameEngine, gameRepository);
    gameController.Run();
    return null;
}

// ------------------------------------------------ L O A D   G A M E ------------------------------------------------

string? LoadGame()
{
    Console.WriteLine("Saved games");
    var saveGameList = gameRepository.GetSaveGames();
    var saveGameListDisplay = saveGameList.Select((s, i) => $"({i + 1})" + " - " + s).ToList();

    if (saveGameListDisplay.Count == 0) return null;
    
    Console.WriteLine(string.Join("\n", saveGameListDisplay));
    
    var userChoice = int.Parse(ConsoleInput.Validate(
        Enumerable.Range(1, saveGameList.Count).Select(n => n.ToString()).ToList(), 
        $"Select game to load (1..{saveGameListDisplay.Count}): ",
        "Error, please try again."));
    
    Guid gameId = saveGameList[userChoice - 1].id;
    Console.WriteLine($"Loading file: {gameId}");
    var gameState = gameRepository.LoadGame(gameId);

    var gameEngine = new UnoGameEngine()
    {
        State = gameState
    };
    
    var gameController = new GameController(gameEngine, gameRepository);
    gameController.Run();

    return null;
}
