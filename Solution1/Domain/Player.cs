namespace Domain;

public class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NickName { get; set; } = default!;
    public EPlayerType PlayerType { get; set; }
    public List<UnoCard> PlayerHand { get; set; } = new List<UnoCard>();
    public int Points { get; set; } = 0;

    public Player(){}

    public Player(string nickName, EPlayerType playerType)
    {
        NickName = nickName;
        PlayerType = playerType;
    }
}