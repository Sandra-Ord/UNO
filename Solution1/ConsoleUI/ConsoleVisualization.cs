using Domain;
using Helpers;

namespace ConsoleUI;

public static class ConsoleVisualization
{
    private const string Separator =
        "------------------------------------------------------------------------------------------";
    
    // ------------------------------------------------- HUMAN MOVE --------------------------------------------------
    
    public static ECardSuit HumanChooseColor()
    {
        Console.Write("\nChoose a color:\n");
        var colors = DisplayColors();
        var validChoices = colors.Keys.ToList();
        return colors[ConsoleInput.Validate(validChoices)];
    }
    
    private static Dictionary<string, ECardSuit> DisplayColors()
    {
        var colors = new Dictionary<string, ECardSuit>();
        for (var i = 0; i < (int)ECardSuit.Wild; i++)
        {
            colors[((ECardSuit) i).Description()] = (ECardSuit) i;
            Console.WriteLine($"({((ECardSuit) i).Description()}) {(ECardSuit) i}");
        }
        return colors;
    }
    
    // ------------------------------------------------- GAME DISPLAY ------------------------------------------------

    private static void UnoHeader()
    {
        Console.WriteLine(Separator);
        ConsoleWriteBetweenSeparator(" U  N  O ");
        Console.WriteLine(Separator);
    }

    public static void WriteConsoleTable(GameState state)
    {
        UnoHeader();
        Console.WriteLine($"Discarded cards: {state.DiscardedCards.Count}");
        Console.WriteLine($"Cards in deck: {state.DeckOfCards.Count}");
        var direction = !state.Reverse ? "Clockwise" : "CounterClockwise";
        Console.WriteLine($"Playing direction: {direction}");
        var playerCards = "Cards in hand:   " + string.Join("   |   ", 
            state.Players.Select(
                player => $"{player.NickName} ({player.PlayerHand.Count.ToString()})").ToList());
        Console.WriteLine(playerCards);
        Console.WriteLine(Separator);
        ConsoleWriteExpectedCard(state);
        Console.Write("\n");
        Console.WriteLine(Separator);
    }
    
    public static void ConsoleWriteRecycleDeck()
    {
        Console.WriteLine();
        Console.WriteLine(Separator);
        ConsoleWriteBetweenSeparator("Discard deck is being recycled!");
        Console.WriteLine(Separator);
        Console.WriteLine();
    }
    
    public static string DisplayUserChoice(Player player, bool newCardAllowed)
    {
        var validChoices = Enumerable.Range(1, player.PlayerHand.Count).Select(n => n.ToString()).ToList();
        
        Console.WriteLine("Choose a card: ");
        ConsoleDisplayPlayerHand(player);
        if (newCardAllowed)
        {
            validChoices.Add("T");
            Console.Write("(T) Take an extra card \n");
        }
        else
        {
            validChoices.Add("S");
            Console.Write("(S) Skip your turn \n");
        }
        return ConsoleInput.Validate(validChoices);
    }
    
    public static void ConsoleWriteActions(GameState state, Dictionary<EActions, bool> actionsMade, Player player)
    {
        if (actionsMade[EActions.NewCardDrawn]) ConsoleWriteBetweenSeparator($"{player.NickName} drew a new card");
        if (actionsMade[EActions.MoveSkipped]) ConsoleWriteBetweenSeparator($"{player.NickName} didn't make a move");
        else ConsoleWriteExpectedCard(state, $"{player.NickName} played card: ");
        Console.Write("\n");
    }
    
    // ------------------------------------------------ DISPLAY CARDS ------------------------------------------------

    private static void ConsoleWriteExpectedCard(GameState state, string text = "Current card: ")
    {
        // Will display the last played card (and in case of a wild card the next expected color).
        Console.Write($"\n{text}");
        ConsoleWriteCard(state.LastPlayedCard!);
        if (state.LastPlayedCard!.CardType == ECardType.Wild)
            Console.Write($" & expected color is {state.LastPlayedColor}");
    }

    private static void ConsoleDisplayPlayerHand(Player player)
    {
        player.PlayerHand.Sort();
        for (var index = 0; index < player.PlayerHand.Count; index++)
        {
            Console.Write("(" + (index + 1) + ") ");
            ConsoleWriteCard(player.PlayerHand[index]);
            Console.Write("   ");
        }
    }
    
    private static void ConsoleWriteCard(UnoCard card)
    {
        Console.ForegroundColor = ConsoleColor.Black;
        Console.BackgroundColor = card.CardSuit switch
        {
            ECardSuit.Red => ConsoleColor.Red,
            ECardSuit.Yellow => ConsoleColor.Yellow,
            ECardSuit.Green => ConsoleColor.Green,
            ECardSuit.Blue => ConsoleColor.Blue,
            ECardSuit.Wild => ConsoleColor.White,
            _ => Console.BackgroundColor
        };
        Console.Write("{" + card.CardValueToString() + "}");
        Console.ResetColor();
    }

    public static void ConsoleWriteBetweenSeparator(string text)
    {
        var textLength = text.Length;
        var firstSeparatorLength = (int) Math.Floor(((decimal)(Separator.Length - 2) - textLength) / 2);
        var secondSeparatorLength = (int) Math.Ceiling(((decimal)(Separator.Length - 2) - textLength) / 2);
        Console.WriteLine(
            $"{new string('-', firstSeparatorLength)} {text} {new string('-', secondSeparatorLength)}");
    }
    
    // -------------------------------------------------- GAME END ---------------------------------------------------
    
    public static void ConsoleWriteWinner(Player winner)
    {
        Console.WriteLine("\n");
        Console.Write(Separator);
        Console.Write("\n");
        Console.WriteLine($"Winner is {winner.NickName}! Congratulations!");
        Console.WriteLine($"Total points: {winner.Points.ToString()}");
        Console.Write(Separator);
        Console.WriteLine("\n");
    }

    public static void FinalCardsInHand(GameState state)
    {
        Console.WriteLine("Final cards left in hands: ");
        foreach (var player in state.Players) DisplayPlayerHand(player);
    }
    
    private static void DisplayPlayerHand(Player player)
    {
        Console.Write($"{player.NickName}'s hand: ");
        ConsoleDisplayPlayerHand(player);
        Console.Write("\n");
    }
}