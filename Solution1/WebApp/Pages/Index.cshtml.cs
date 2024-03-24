using DAL;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IGameRepository _gameRepository;

    public IndexModel(ILogger<IndexModel> logger, IGameRepository gameRepository)
    {
        _logger = logger;
        _gameRepository = gameRepository;
    }

    public int Count { get; set; }

    public void OnGet()
    {
        Count = _gameRepository.GetSaveGames().Count();
    }
}