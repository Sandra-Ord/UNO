// See https://aka.ms/new-console-template for more information

using ConsoleApp1;
using DAL;
using UnoEngine;
using ConsoleUI;
using Microsoft.EntityFrameworkCore;


/*var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite("Data Source=app.db")
    .EnableDetailedErrors()
    .EnableSensitiveDataLogging()
    .Options;

using var db = new AppDbContext(contextOptions);
db.Database.Migrate();*/

IGameRepository gameRepository = new GameRepositoryFileSystem();
//IGameRepository gameRepository = new GameRepositoryEF(db);



// ----------------------------------------------------- M A I N -----------------------------------------------------
var mainMenu = ProgramMenus.GetMainMenu(NewGame, LoadGame);
mainMenu.Run();

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

    Guid gameId;
    while (true)
    {
        Console.WriteLine(string.Join("\n", saveGameListDisplay));
        Console.Write($"Select game to load (1..{saveGameListDisplay.Count}): ");
        var userChoiceStr = Console.ReadLine();
        if (int.TryParse(userChoiceStr, out var userChoice))
        {
            if (userChoice < 1 || userChoice > saveGameListDisplay.Count)
            {
                Console.WriteLine("Enter a valid option");
                continue;
            }
            gameId = saveGameList[userChoice - 1].id;
            Console.WriteLine($"Loading file: {gameId}");
            break;
        }
        Console.WriteLine("Parse error...");
    }

    var gameState = gameRepository.LoadGame(gameId);

    var gameEngine = new UnoGameEngine()
    {
        State = gameState
    };
    
    var gameController = new GameController(gameEngine, gameRepository);
    gameController.Run();

    return null;
}
