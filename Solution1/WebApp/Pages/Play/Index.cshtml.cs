using Microsoft.AspNetCore.Mvc.RazorPages;
using DAL;
using Domain;
using Microsoft.AspNetCore.Mvc;
using UnoEngine;

namespace WebApp.Pages.Play;

public class Index : PageModel
{
    private readonly IGameRepository _gameRepository;
    public UnoGameEngine Engine = default!;
    
    [BindProperty(SupportsGet = true)]
    public Guid PlayerId { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public Guid GameId { get; set; }
    
    [BindProperty] 
    public string? ActionButton { get; set; }
    
    [BindProperty(SupportsGet = true)] 
    public string? SelectCard { get; set; }
    
    [BindProperty] 
    public string? SelectColor { get; set; }

    [BindProperty(SupportsGet = true)] 
    public bool NewCardTaken { get; set; }

    public Index(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }
    
    public void OnGet()
    {
        var gameState = _gameRepository.LoadGame(GameId);
        Engine = new UnoGameEngine() {State = gameState};
    }


    public IActionResult OnPost(Guid gameId, Guid playerId)
    {
        var gameState = _gameRepository.LoadGame(GameId);
        Engine = new UnoGameEngine() {State = gameState};

        switch (ActionButton)
        {
            case "Back":
            {
                SelectCard = null;
                SelectColor = null;
                break;
            }case "NewCard":
            {
                if (Engine.NewCardAllowed()) Engine.TakeCard();
                NewCardTaken = true;
                SelectCard = null;
                break;
            }
            case "SkipTurn":
            {
                Engine.NextPlayer();
                NewCardTaken = false;
                SelectCard = null;
                break;
            }
            case "PlayerSkipped":
            {
                Engine.ApplyEffectsOfSkipMove();
                break;
            }
            case "AIPlayCard":
            {
                Engine.MakeAiMove();
                Engine.ApplyEffectsOfMove();
                break;
            }
            case "HumanPlayCard":
            {
                if (SelectCard == null || !int.TryParse(SelectCard, out int index)) break;
                var possibleMoves = Engine.PossibleMoves(Engine.GetPlayer(PlayerId));
                if (!(0 <= index && index <= possibleMoves.Count - 1)) break; // Index is out of bounds
                
                var playCard = possibleMoves[index];
                if (playCard.CardSuit == ECardSuit.Wild) break;
                
                Engine.PlayColorCard(playCard);
                Engine.ApplyEffectsOfMove();
                SelectCard = null;
                NewCardTaken = false;
                break;
            }
            case "ChooseColor":
            {
                if (SelectCard == null || SelectColor == null || !int.TryParse(SelectCard, out int cardIndex) ||
                    !int.TryParse(SelectColor, out int colorIndex)) break;  // Input is invalid.
                SelectCard = null;
                var possibleMoves = Engine.PossibleMoves(Engine.GetPlayer(PlayerId));
                if (!(0 <= cardIndex && cardIndex <= possibleMoves.Count - 1 && 
                      0 <= colorIndex && colorIndex <= (int)ECardSuit.Wild)) break; // Indices are out of bounds, go back to choosing card.
                
                var playCard = possibleMoves[cardIndex];
                if (playCard.CardSuit != ECardSuit.Wild) break; // Something went wrong, go back to choose card
                
                Engine.PlayWildCard(playCard, (ECardSuit) colorIndex);
                Engine.ApplyEffectsOfMove();
                NewCardTaken = false;
                break;
            }
        }
        
        _gameRepository.SaveGame(GameId, gameState);

        return RedirectToPage("./Index", 
            new
            {
                GameId, 
                PlayerId,
                NewCardTaken,
                SelectCard
            });
    }
}