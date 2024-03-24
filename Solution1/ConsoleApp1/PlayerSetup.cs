using Domain;
using Helpers;
using UnoEngine;

namespace ConsoleApp1;

public static class PlayerSetup
{
    
    private static readonly Dictionary<string, EPlayerType> TypeChoices= new Dictionary<string, EPlayerType>
    {
        ["A"] = EPlayerType.AI,
        ["H"] = EPlayerType.Human
    };
    
    public static void ConfigurePlayers(UnoGameEngine gameEngine)
    {
        Console.WriteLine();
        var playerCount = int.Parse(ConsoleInput.Validate(
            Enumerable.Range(UnoGameEngine.MinPlayerAmount, UnoGameEngine.MaxPlayerAmount - 1)
                .Select(n => n.ToString()).ToList(),
            $"Enter the amount of players for the game ({UnoGameEngine.MinPlayerAmount} - {UnoGameEngine.MaxPlayerAmount}): ",
            $"Input not in the range [{UnoGameEngine.MinPlayerAmount} - {UnoGameEngine.MaxPlayerAmount}]."));
        Console.WriteLine();
        
        for (var i = 0; i < playerCount; i++)
        {
            Console.WriteLine($"Player {i + 1} types:");
            Console.WriteLine("(A) - AI  |  (H) - human");
            var playerType = ConsoleInput.Validate(TypeChoices.Keys.ToList());
            
            var defaultName = ((playerType == "A") ? "AI" : "human") + (i + 1); 
            var playerName = ConsoleInput.Validate(
                new List<string>(), 
                $"Nickname for Player {i + 1} ({playerType}) (min 1 letter) [{defaultName}]: ", 
                "Error, please try again", 
                defaultName,
                false);

            gameEngine.State.Players.Add(new Player(playerName, TypeChoices[playerType]));
            Console.WriteLine();
        }
    }
}