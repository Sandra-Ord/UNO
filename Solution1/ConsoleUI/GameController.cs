using UnoEngine;
using DAL;

namespace ConsoleUI;
using Domain;

public class GameController
{
    private readonly UnoGameEngine _engine;
    private readonly IGameRepository _repository;

    public GameController(UnoGameEngine engine, IGameRepository repository)
    {
        _engine = engine;
        _repository = repository;
    }
    
    public void Run()
    {
        PressEnter();
        while (!_engine.GameFinished())
        {
            Console.Clear();
            ConsoleVisualization.WriteConsoleTable(_engine.State);
            var player = _engine.GetActivePlayer();

            // ---------------- CARD HAS BEEN PLAYED WHICH WILL CAUSE THE NEXT PLAYER TO BE SKIPPED ------------------
            if (_engine.PlayerWillBeSkipped())
            {
                ConsoleVisualization.ConsoleWriteBetweenSeparator($"{player.NickName}'s turn is being skipped");
                _engine.ApplyEffectsOfSkipMove();
                
                if (!SaveAndContinue()) break;
                Console.Clear();
                PressEnter();
                continue;
            }

            ConsoleVisualization.ConsoleWriteBetweenSeparator($"{player.NickName}'s turn");
            // --------------------------------------------- MAKE A MOVE ---------------------------------------------
            if (_engine.IsHuman(player))
            {
                var actionsMade = MakeHumanMove(player);
                ConsoleVisualization.ConsoleWriteActions(_engine.State, actionsMade, player);
            }
            else
            {
                Console.Write($"{player.NickName}'s hand: {player.PlayerHand.Count} cards\n");
                var actionsMade = _engine.MakeAiMove();
                ConsoleVisualization.ConsoleWriteActions(_engine.State, actionsMade, player);
            }

            _engine.ApplyEffectsOfMove();
            if (_engine.DeckRecyclingAllowed()) ConsoleVisualization.ConsoleWriteRecycleDeck();
            
            if (!SaveAndContinue()) break;
            Console.Clear();
            PressEnter();
        }

        if (!_engine.GameFinished()) return; // Game has not finished, loop was exited due to !saveAndContinue
        
        ConsoleVisualization.FinalCardsInHand(_engine.State);
        ConsoleVisualization.ConsoleWriteWinner(_engine.State.Winner!);
    }
    
    // ------------------------------------------------- HUMAN MOVE --------------------------------------------------

    private Dictionary<EActions, bool> MakeHumanMove(Player player)
    {
        var actionsMade = UnoGameEngine.ActionDictionary();
        UnoCard playCard;
        var newCardAllowed = _engine.NewCardAllowed();
        do
        {
            var playCardIndex = ConsoleVisualization.DisplayUserChoice(player, newCardAllowed);
            if (newCardAllowed && playCardIndex == "T")
            {
                _engine.TakeCard();
                actionsMade[EActions.NewCardDrawn] = true;
                newCardAllowed = false;
                playCardIndex = ConsoleVisualization.DisplayUserChoice(player, newCardAllowed);
            }
            if (playCardIndex == "S")  // Move skipped
            {
                actionsMade[EActions.MoveSkipped] = true;
                break; 
            }

            playCard = player.PlayerHand[int.Parse(playCardIndex) - 1];
            if (_engine.MoveAllowed(playCard, player.PlayerHand))
            {
                if (playCard.CardSuit == ECardSuit.Wild) _engine.PlayWildCard(playCard, ConsoleVisualization.HumanChooseColor());
                else _engine.PlayColorCard(playCard);
                actionsMade[EActions.CardPlayed] = true;  // currently won't be used
                break;
            }
            Console.WriteLine($"Forbidden move {playCard} played on top {_engine.State.LastPlayedCard}");
        } while (!_engine.MoveAllowed(playCard, player.PlayerHand));
        return actionsMade;
    }
    
    // ------------------------------------------------- GAME CONTROL ------------------------------------------------
    
    private bool SaveAndContinue()
    {
        _repository.SaveGame(_engine.State.Id, _engine.State);
        Console.Write("\nState saved. Continue (Y/N) [Y]: ");
        var continueAnswer = Console.ReadLine()?.Trim().ToUpper();
        return continueAnswer != "N";
    }

    private void PressEnter()
    {
        Console.WriteLine();
        Console.WriteLine($"Press enter key to go to {_engine.GetActivePlayer().NickName}'s move: ");
        Console.ReadKey();
        Console.Clear();
    }
}