using Helpers;

namespace Domain;

public class UnoCard: IComparable<UnoCard?>
{
    /*
     * Each card consists of ...
     *      a suit - red, yellow, green, blue or wild,
     *      a value - numbers 0...9, skip, reverse, draw 2, draw 4.
     * Each card is also assigned a type:
     *      Wild if the suit is wild,
     *      Action if the value is either skip, reverse or draw 2,
     *      Number if the value is 0...9
     * Based on type the card is algo assigned the amount of points it is worth in the end of the game:
     *      Wild - 50 points,
     *      Action - 20 points,
     *      Number - faceValue points
     */
    
    public ECardSuit CardSuit { get; set; }
    public ECardValue CardValue { get; set; }
    public ECardType CardType { get; set; }
    public readonly int CardPoints;
    public bool ActionTaken { get; set; } = false;
    
    private static readonly List<ECardValue> WildValues = new() {ECardValue.WildChangeColor, ECardValue.WildDrawFour};
    private static readonly List<ECardValue> ActionValues = new() {ECardValue.ActionReverse, ECardValue.ActionSkip, ECardValue.ActionDrawTwo};

    public UnoCard(ECardSuit cardSuit, ECardValue cardValue)
    {   
        // ALl values can't be paired with all types, check if card type is wild and value isn't and vice versa.
        if ((WildValues.Contains(cardValue) && cardSuit != ECardSuit.Wild) || 
            (cardSuit == ECardSuit.Wild && !WildValues.Contains(cardValue)))
            throw new Exception("Illegal card: " + cardSuit + " " + cardValue);
        
        CardSuit = cardSuit;
        CardValue = cardValue;

        if (cardSuit == ECardSuit.Wild)
        {
            CardType = ECardType.Wild;
            CardPoints = 50;
        } 
        else if (ActionValues.Contains(CardValue))
        {
            CardType = ECardType.Action;
            CardPoints = 20;
        }
        else
        {
            CardType = ECardType.Number;
            CardPoints = (int) CardValue;
            ActionTaken = true;
        }
    }

    public void ResetCard()
    {
        ActionTaken = CardType == ECardType.Number; // If card type is number, action taken is true, else false;
    }

    public override string ToString() => "{" + CardSuitToString() + CardValueToString() + "}";

    public string CardSuitToString() => CardSuit.Description();

    public string CardValueToString() => CardValue.Description();

    public string GetCardPicturePath()
    {
        var suit = CardSuit != ECardSuit.Wild ? CardSuit.Description() : "W";
        var value = (CardValue != ECardValue.ActionReverse && CardValue != ECardValue.WildChangeColor && CardValue != ECardValue.ActionSkip)
            ? CardValue.Description()
            : (CardValue == ECardValue.WildChangeColor ? "Wild" 
                : (CardValue == ECardValue.ActionReverse ? "Reverse" : "Skip"));
        return  "Pictures/UnoCards/" + suit + "/" + value + ".png";
    }
    
    public static string GetDefaultCardPicturePath(int eCardSuit) => "Pictures/UnoCards/Default/" + ((ECardSuit) eCardSuit).Description() + ".png";

    public static string GetDefaultCardPicturePath() => "Pictures/UnoCards/Default/Deck.png";
    
    public int CompareTo(UnoCard? otherCard)
    {
        if (otherCard == null) throw new ArgumentNullException(nameof(otherCard));
        
        // Check which card type is stronger (wild > action > number);
        if ((int)CardType < (int)otherCard.CardType) return -1;
        if ((int)CardType > (int)otherCard.CardType) return 1;
        
        // Same card type, check the face value;
        if (CardType == ECardType.Number)
        {
            // If cards are number cards, then the bigger the number the weaker the card - gives winner more points
            if ((int)CardValue > (int)otherCard.CardValue) return -1;
            if ((int)CardValue < (int)otherCard.CardValue) return 1;
        }
        if ((int)CardValue < (int)otherCard.CardValue) return -1;
        if ((int)CardValue > (int)otherCard.CardValue) return 1;
        
        // Card type and card value is the same - color doesn't matter
        return 0;
    }
}