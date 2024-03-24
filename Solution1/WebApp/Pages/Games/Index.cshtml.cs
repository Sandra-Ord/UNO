using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DAL;
using Domain;
using Domain.Database;
using Helpers;
using Player = Domain.Database.Player;

namespace WebApp.Pages_Games
{
    public class IndexModel : PageModel
    {
        public readonly IGameRepository _gameRepository;

        public IndexModel(IGameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }

        public IList<Game> Games { get;set; } = default!;

        public Task OnGetAsync()
        {

            var games = _gameRepository.GetSaveGames();
            Games = new List<Game>();
            foreach (var gameInfo in games)
            {
                var gameState = _gameRepository.LoadGame(gameInfo.id);

                var game = new Game()
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
                Games.Add(game);
            }

            Games = Games.OrderByDescending(g => g.UpdatedAt).ToList();

            return Task.CompletedTask;
        }
    }
}
