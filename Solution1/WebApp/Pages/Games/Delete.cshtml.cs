using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL;
using Domain.Database;
using Helpers;

namespace WebApp.Pages_Games
{
    public class DeleteModel : PageModel
    {
        public readonly IGameRepository GameRepository;

        public DeleteModel(IGameRepository gameRepository)
        {
            GameRepository = gameRepository;
        }

        [BindProperty]
        public Game Game { get; set; } = default!;

        public Task<IActionResult> OnGet(Guid? id)
        {
            if (id == null || GameRepository.GetSaveGames().Count == 0)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }
            
            var gameExists = GameRepository.GetSaveGames().Any(t => t.id == id);
            
            if (gameExists == false)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }
            else
            {
                var gameInfo = GameRepository.GetSaveGames().FirstOrDefault(t => t.id == id);

                var gameState = GameRepository.LoadGame(gameInfo.id);

                Game = new Game()
                {
                    CreatedAt = gameInfo.created,
                    UpdatedAt = gameInfo.updated,
                    State = System.Text.Json.JsonSerializer.Serialize(gameState, JsonHelpers.JsonSerializerOptions),
                    Players = gameState.Players.Select(p => new Player()
                    {
                        Id = p.Id,
                        NickName = p.NickName,
                        PlayerType = p.PlayerType,
                        GameId = gameInfo.id,
                    }).ToList(),
                };
            }
            return Task.FromResult<IActionResult>(Page());
        }

        public Task<IActionResult> OnPost(Guid? id)
        {
            if (id == null || GameRepository.GetSaveGames().Count == 0)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }
            var gameExists = GameRepository.GetSaveGames().Any(t => t.id == id);

            if (gameExists)
            {
                GameRepository.DeleteGame(id);
            }

            return Task.FromResult<IActionResult>(RedirectToPage("./Index"));
        }
    }
}
