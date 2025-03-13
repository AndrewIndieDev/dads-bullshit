using AndrewDowsett.Utility;
using System;
using System.Collections.Generic;

public class Deck
{
    public static Dictionary<string, Deck> FindDeck;
    public Deck(string uniqueId)
    {
        this.uniqueId = uniqueId;
        if (FindDeck == null)
            FindDeck = new();
        FindDeck.Add(uniqueId, this);
        SetupDeck();
    }

    private string uniqueId;
    private List<Card> cards;

    public Action onDeckSetup;
    public Action onDeckCleared;
    public Action onDeckShuffled;
    public Action<Card> onCardRemoved;
    public Action<Card> onCardAdded;

    public void SetupDeck()
    {
        if (cards != null)
            ClearDeck();
        else
            cards = new();
        onDeckSetup?.Invoke();
    }
    public void ClearDeck()
    {
        cards.Clear();
        onDeckCleared?.Invoke();
    }
    public void ShuffleDeck()
    {
        cards.Shuffle();
        onDeckShuffled?.Invoke();
    }
    public Card PullCard()
    {
        return PullCard(0);
    }
    public Card PullCard(int index)
    {
        if (cards.Count <= index)
            return null;
        Card card = cards[index];
        cards.RemoveAt(index);
        onCardRemoved?.Invoke(cards[index]);
        return card;
    }
    public Card PullCard(Card card)
    {
        return PullCard(cards.IndexOf(card));
    }
    public void AddCard(Card card)
    {
        cards.Add(card);
        onCardAdded?.Invoke(card);
    }
}
