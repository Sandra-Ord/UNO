namespace Domain;

public class GameState
{

    public Guid Id { get; set; } = Guid.NewGuid();
    
    public List<UnoCard> DeckOfCards { get; set; } = new List<UnoCard>();

    public List<UnoCard> DiscardedCards { get; set; } = new List<UnoCard>();
    
    public  UnoCard? LastPlayedCard { get; set; }

    public ECardSuit LastPlayedColor { get; set; }
    
    public bool Reverse { get; set; } = false;

    public int ActivePlayer { get; set; } = 0;

    public Player? Winner { get; set; }

    public bool DeckReplaced { get; set; } = false;
    
    public List<Player> Players { get; set; } = new List<Player>();
}