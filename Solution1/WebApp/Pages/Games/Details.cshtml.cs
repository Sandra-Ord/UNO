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
    public class DetailsModel : PageModel
    {
        public readonly IGameRepository GameRepository;

        public DetailsModel(IGameRepository gameRepository)
        {
            GameRepository = gameRepository;
        }

        public Game Game { get; set; } = default!; 

        public Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null || GameRepository.GetSaveGames().Count == 0)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }

            if (GameRepository.GetSaveGames().Any(t => t.id == id) == false)
            {
                return Task.FromResult<IActionResult>(NotFound());
            }

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
            return Task.FromResult<IActionResult>(Page());
        }
    }
}
