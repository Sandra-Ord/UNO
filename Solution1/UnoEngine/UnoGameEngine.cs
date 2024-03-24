using Domain;

namespace UnoEngine;

public class UnoGameEngine
{

    public GameState State { get; set; } = new GameState();

    private readonly List<ECardValue> _missMoveValues = new List<ECardValue>()
        {ECardValue.ActionSkip, ECardValue.ActionDrawTwo, ECardValue.WildDrawFour };

    private readonly List<ECardValue> _extraCardsValues = new List<ECardValue>()
        {ECardValue.ActionDrawTwo, ECardValue.WildDrawFour};

    public static readonly int MinPlayerAmount = 2;
    public static readonly int MaxPlayerAmount = 10;
    public readonly int InitialHandSize = 7;

    private Random Rng { get; set; } = new Random();

    public void SetUpGame()
    {
        InitializeFullDeck();
        ShuffleDeck();
        DealPlayerHands();
        GetFirstCard();
    }
    
    // --------------------------------------------- PLAYER HELPED METHODS ------------------------------------------

    public Player GetActivePlayer() => State.Players[State.ActivePlayer];

    public Player GetPlayer(Guid playerId) => State.Players.First(player => player.Id == playerId);
    
    public bool IsHuman(Player player) => player.PlayerType == EPlayerType.Human;
    
    public bool IsAi(Player player) => player.PlayerType == EPlayerType.AI;

    // ------------------------------------------------- MAKE A MOVE ------------------------------------------------

    public static Dictionary<EActions, bool> ActionDictionary() => new Dictionary<EActions, bool>()
        {
            [EActions.NewCardDrawn] = false,
            [EActions.CardPlayed] = false,
            [EActions.MoveSkipped] = false,
        };
    
    public Dictionary<EActions, bool> MakeAiMove()
    {
        var actionsMade = ActionDictionary(); // Can be used for displaying later.
        var possibleMoves = PossibleMoves(GetActivePlayer());
        
        // Sorted from weakest to strongest: 9...0 cards, action cards, wild cards.
        possibleMoves.Sort();
        if (possibleMoves.Count == 0)  // No possible moves.
        {
            if (!NewCardAllowed())  // Can't take new card.
            {
                actionsMade[EActions.MoveSkipped] = true;
                return actionsMade;
            }
            
            var newCard = TakeCard();
            actionsMade[EActions.NewCardDrawn] = true;

            if (MoveAllowed(newCard, GetActivePlayer().PlayerHand))
            {
                if (newCard.CardSuit == ECardSuit.Wild) PlayWildCard(newCard, AiChooseColor());
                else PlayColorCard(newCard);
                State.LastPlayedColor = newCard.CardSuit != ECardSuit.Wild ? newCard.CardSuit : AiChooseColor();
                actionsMade[EActions.CardPlayed] = true;
            } else actionsMade[EActions.MoveSkipped] = true;
        }
        else
        {
            var playCard = (State.DeckOfCards.Count <= 2 * InitialHandSize || State.DeckReplaced) 
                ? possibleMoves.Last() : possibleMoves.First();
            if (playCard.CardSuit == ECardSuit.Wild) PlayWildCard(playCard, AiChooseColor());
            else PlayColorCard(playCard);
            State.LastPlayedColor = playCard.CardSuit != ECardSuit.Wild ? playCard.CardSuit : AiChooseColor();
            actionsMade[EActions.CardPlayed] = true;
        }
        return actionsMade;
    }
    
    private ECardSuit AiChooseColor()
    {
        var colorsInHand = new List<ECardSuit>();
        foreach (var card in GetActivePlayer().PlayerHand
                     .Where(card => card.CardSuit != ECardSuit.Wild && !colorsInHand.Contains(card.CardSuit)))
            colorsInHand.Add(card.CardSuit);

        return colorsInHand is []
            ? (ECardSuit)Rng.Next((int)ECardSuit.Wild)
            : colorsInHand[Rng.Next(colorsInHand.Count)];
    }

    // ---------------------------------------------- INFO ABOUT MOVES -----------------------------------------------
    public List<UnoCard> PossibleMoves(Player player) => State.LastPlayedColor == ECardSuit.Wild 
        ? player.PlayerHand 
        : player.PlayerHand.FindAll(card => MoveAllowed(card, player.PlayerHand));

    public bool MoveAllowed(UnoCard card, List<UnoCard> hand)
    {
        if (State.LastPlayedColor == ECardSuit.Wild) return true; // only possible when the very 1st card is change color
        if (card.CardValue == ECardValue.WildChangeColor) return true;
        if (card.CardValue == ECardValue.WildDrawFour) return hand.All(eachCard => eachCard.CardSuit != State.LastPlayedColor);
        if (card.CardValue == State.LastPlayedCard!.CardValue) return true;
        return card.CardSuit == State.LastPlayedColor;
    }
    
    // ------------------------------------------------ MOVE ELEMENTS ------------------------------------------------
    public void ApplyEffectsOfMove()
    {
        /*
         * Checks if player has won,
         * if player should take a new card after the move,
         * check if the direction should be swapped,
         * goes to the next player
         */
        if (PlayerHasWon()) State.Winner = GetActivePlayer();
        if (NewCardAfterMoveAllowed()) TakeCard();
        if (DirectionSwapAllowed())
        {
            DirectionSwap();
            if (State.Players.Count == 2) NextPlayer();
            State.LastPlayedCard!.ActionTaken = true;
        }
        if (DeckRecyclingAllowed()) RecycleDiscardedCards();
        NextPlayer();
        if (State.Winner == null) return;
        
        if (PlayerGetsExtraCards()) TakeMultipleCards(); // Finish the effect of the last move
        CalculateWinnerTotalPoints();
    }

    public void ApplyEffectsOfSkipMove()
    {
        if (!PlayerWillBeSkipped()) return;
        TakeMultipleCards();
        NextPlayer();
        State.LastPlayedCard!.ActionTaken = true;
    }

    public void PlayColorCard(UnoCard card)
    {
        PlayCardDiscardLast(card);
        State.LastPlayedColor = card.CardSuit;
    }

    public void PlayWildCard(UnoCard card, ECardSuit color)
    {
        PlayCardDiscardLast(card);
        State.LastPlayedColor = color;
    }

    private void PlayCardDiscardLast(UnoCard card)
    {
        /*
         * Removes the card from hand,
         * discards the previous LastPlayed card and resets it (actionTaken to its original value),
         * sets playCard as the LastPlayedCard
         */
        GetActivePlayer().PlayerHand.Remove(card);
        State.DiscardedCards.Add(State.LastPlayedCard!);
        State.LastPlayedCard!.ResetCard();
        State.LastPlayedCard = card;
    }

    private void DirectionSwap() => State.Reverse = !State.Reverse;

    public void NextPlayer()
    {
        if (State.Reverse == false) State.ActivePlayer = (State.ActivePlayer + 1) % State.Players.Count;
        else
        {
            if (State.ActivePlayer == 0) State.ActivePlayer = State.Players.Count - 1;
            else State.ActivePlayer--;
        }
    }

    // ---------------------------------------------- TAKE CARDS ----------------------------------------------
    private void TakeMultipleCards()
    {
        if (State.LastPlayedCard!.CardValue == ECardValue.ActionDrawTwo) TakeMultipleCards(2);
        else if (State.LastPlayedCard.CardValue == ECardValue.WildDrawFour) TakeMultipleCards(4);
    }

    private void TakeMultipleCards(int amount) 
    {
        for (var i = 0; i < amount; i++) if (NewCardAllowed()) TakeCard();
    }
    
    public UnoCard TakeCard()
    {
        // ----------------------- Only use with a check if there is enough cards in the deck. -----------------------
        var topOfDeck = State.DeckOfCards.Last();
        GetActivePlayer().PlayerHand.Add(topOfDeck);
        State.DeckOfCards.Remove(topOfDeck);
        return topOfDeck;
    }
    
    // ------------------------------------------- BOOLEAN INFO ABOUT GAME -------------------------------------------
    private bool NewCardAfterMoveAllowed() => !State.DeckReplaced &&
                                              GetActivePlayer().PlayerHand.Count < InitialHandSize &&
                                              State.DeckOfCards.Count != 0 &&
                                              State.Winner == null;

    private bool DirectionSwapAllowed() => !State.LastPlayedCard!.ActionTaken &&
                                           State.LastPlayedCard!.CardValue == ECardValue.ActionReverse;
    
    public bool NewCardAllowed() => State.DeckOfCards.Count > 0;
    
    public bool PlayerWillBeSkipped() => !State.LastPlayedCard!.ActionTaken && 
                                         _missMoveValues.Contains(State.LastPlayedCard!.CardValue);
    
    public bool PlayerGetsExtraCards() => !State.LastPlayedCard!.ActionTaken &&
                                          _extraCardsValues.Contains(State.LastPlayedCard.CardValue);
    
    public bool DeckRecyclingAllowed() => State.Winner == null &&
                                          State.DeckOfCards.Count == 0 &&
                                          State.Players.Sum(eachPlayer => PossibleMoves(eachPlayer).Count) == 0;

    private bool PlayerHasWon() => GetActivePlayer().PlayerHand.Count == 0;

    public bool GameFinished() => State.Winner != null;

    // ------------------------------------------------ DECK METHODS -------------------------------------------------
    private void InitializeFullDeck()
    {
        /*
         * Full deck has:
         * 1 set of 0 in each color,
         * 2 sets of numbers + skip + reverse + draw 2 in each color,
         * 4 wild any, 4 wild draw 4 cards.
         *
         * 2nd for loop creates 4 cards of each
         */
        CreateColorCards();
        CreateWildCards();
    }

    private void CreateColorCards()
    {
        /*
         * Create:
         * 1 set of 0 value cards in each color,
         * 2 sets of 1-9 + skip + reverse + drawTwo cards in each color.
         *
         * Variable numberOfEach is set to 1 for creating 0 value cards and then to 2 to create the rest.
         */
        var numberOfEach = 1;
        for (var cardValue = 0; cardValue < (int) ECardValue.WildChangeColor; cardValue++)
        {
            for (var number = 0; number < numberOfEach; number++)
            {
                for (var cardSuit = 0; cardSuit < (int)ECardSuit.Wild; cardSuit++)
                {
                    State.DeckOfCards.Add(new UnoCard((ECardSuit)cardSuit, (ECardValue)cardValue));
                }
            }
            numberOfEach = 2;
        }
    }

    private void CreateWildCards()
    {
        /*
         * Create:
         * 4 wild change color cards,
         * 4 wild draw four cards.
         */
        for (var cardValue = (int) ECardValue.WildChangeColor; cardValue < (int) ECardValue.WildDrawFour + 1; cardValue++)
        {
            for (var amount = 0; amount < 4; amount++)
            {
                State.DeckOfCards.Add(new UnoCard(ECardSuit.Wild, (ECardValue)cardValue));
            }
        }
    }

    private void ShuffleDeck()
    {
        /*
         * Remove a card at a random position from the DeckOfCards and put it to the last position of shuffleDeck.
         * Rng.Next(Count) chooses a random number from 0 -> Count-1,
         *      since Count gets smaller after each iteration, card at the position is guaranteed.
         * Replace Game deck with helper deck.
         */
        var shuffleDeck = new List<UnoCard>();
        while (State.DeckOfCards.Count > 0)
        {
            var randomPosition = Rng.Next(State.DeckOfCards.Count);
            shuffleDeck.Add(State.DeckOfCards[randomPosition]);
            State.DeckOfCards.RemoveAt(randomPosition);
        }
        State.DeckOfCards = shuffleDeck;
    }

    private void RecycleDiscardedCards()
    {
        // ------------------------ Should only be used if no one has a possible move to make ------------------------
        if (State.DeckOfCards.Count != 0) return; // Do not recycle if cards are not up !
        State.DeckOfCards = State.DiscardedCards;
        State.DiscardedCards = new List<UnoCard>();
        ShuffleDeck();
        State.DeckReplaced = true;
    }

    // ---------------------------------------------- GAME PREPARATIONS ----------------------------------------------
    private void DealPlayerHands()
    {
        foreach (var player in State.Players)
        {
            for (var i = 0; i < InitialHandSize; i++)
            {
                player.PlayerHand.Add(State.DeckOfCards.Last()); 
                State.DeckOfCards.RemoveAt(State.DeckOfCards.Count - 1);
            }
        }
    }

    private void GetFirstCard()
    {
        // Get the first card of the discard pile.
        if (State.DeckOfCards.Count == 0) return;

        var card = State.DeckOfCards.First();
        do
        {
            State.DeckOfCards.RemoveAt(0);
            if (card.CardValue == ECardValue.WildDrawFour) // Forbidden 1st card - +4
                State.DeckOfCards.Add(card);
        } 
        while (card.CardValue == ECardValue.WildDrawFour);

        State.LastPlayedCard = card;
        if (State.LastPlayedCard.CardValue == ECardValue.ActionReverse)
        {
            // Start moving counterclockwise instead, 1st player is not the default 0.
            State.Reverse = true;
            NextPlayer();
        }
        
        // If 1st card in discard pile is Wild (WildChangeColor) card, then next player can choose any card.
        // In other cases, if a player plays a wild card, then they will get to set the lastPlayedColor.
        State.LastPlayedColor = card.CardSuit;
    }
    
    // ------------------------------------------------ GAME WINNER --------------------------------------------------

    private void CalculateWinnerTotalPoints()
    {
        if (State.Winner == null) return;
        State.Winner!.Points = State.Players.Sum(player => player.PlayerHand.Sum(card => card.CardPoints));
    }
}