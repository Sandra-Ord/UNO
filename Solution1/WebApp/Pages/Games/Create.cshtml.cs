using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DAL;
using Domain;
using Domain.Database;
using UnoEngine;
using Player = Domain.Player;

namespace WebApp.Pages_Games
{
    public class CreateModel : PageModel
    {
        private readonly IGameRepository _gameRepository;

        public CreateModel(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        
        public IActionResult OnGet()
        {
            return Page();
        }
        
        
        [BindProperty(SupportsGet = true)]
        public int? PlayerCount { get; set; }

        [BindProperty]
        public Game Game { get; set; } = default!;

        [BindProperty] 
        public string? PlayAs { get; set; }
        
        [BindProperty] 
        public string? PlayerType0 { get; set; }
        [BindProperty] 
        public string? PlayerName0 { get; set; }

        [BindProperty] 
        public string? PlayerType1 { get; set; }
        [BindProperty] 
        public string? PlayerName1 { get; set; }
        
        [BindProperty] 
        public string? PlayerType2 { get; set; }
        [BindProperty] 
        public string? PlayerName2 { get; set; }
        
        [BindProperty] 
        public string? PlayerType3 { get; set; }
        [BindProperty] 
        public string? PlayerName3 { get; set; }
        
        [BindProperty] 
        public string? PlayerType4 { get; set; }
        [BindProperty] 
        public string? PlayerName4 { get; set; }
        
        [BindProperty] 
        public string? PlayerType5 { get; set; }
        [BindProperty] 
        public string? PlayerName5 { get; set; }
        
        [BindProperty] 
        public string? PlayerType6 { get; set; }
        [BindProperty] 
        public string? PlayerName6 { get; set; }
        
        [BindProperty] 
        public string? PlayerType7 { get; set; }
        [BindProperty] 
        public string? PlayerName7 { get; set; }
        
        [BindProperty] 
        public string? PlayerType8 { get; set; }
        [BindProperty] 
        public string? PlayerName8 { get; set; }
        
        [BindProperty] 
        public string? PlayerType9 { get; set; }
        [BindProperty] 
        public string? PlayerName9 { get; set; }

        
        public Task<IActionResult> OnPost()
        {
            var playerTypes = new List<string?>()
            {
                PlayerType0, PlayerType1, PlayerType2, PlayerType3, PlayerType4, PlayerType5, 
                PlayerType6, PlayerType7, PlayerType8, PlayerType9
            };
            var playerNames = new List<string?>()
            {
                PlayerName0, PlayerName1, PlayerName2, PlayerName3, PlayerName4, PlayerName5, 
                PlayerName6, PlayerName7, PlayerName8, PlayerName9
            };

            if (playerTypes.Count(x => x != null) < 2 || playerTypes.Count(x => x != null) > 10) return Task.FromResult<IActionResult>(Page());
            
            var gameEngine = new UnoGameEngine();
            
            for (var i = 0; i < PlayerCount; i++)
            {
                EPlayerType playerType;
                if (playerTypes[i] == "AI") playerType = EPlayerType.AI;
                else if (playerTypes[i] == "Human") playerType = EPlayerType.Human;
                else continue;
                
                string nickName = playerNames[i] ?? playerTypes[i] + (i + 1);
                gameEngine.State.Players.Add(new Player(nickName, playerType));
            }
            
            gameEngine.SetUpGame();

            _gameRepository.SaveGame(gameEngine.State.Id, gameEngine.State);

            if (PlayAs != null && int.TryParse(PlayAs, out int index))
            {
                if (playerTypes[index] != null)
                {
                    return Task.FromResult<IActionResult>(RedirectToPage("/Play/Index", new
                    {
                        GameId = gameEngine.State.Id,
                        PlayerId = gameEngine.State.Players[index].Id
                    }));
                }
            }

            return Task.FromResult<IActionResult>(RedirectToPage("./Index"));
        }
    }
}
