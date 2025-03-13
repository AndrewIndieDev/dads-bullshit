using UnityEditor.Networking.PlayerConnection;
using UnityEngine;

// A lobby of 8 players would have:
// 104 cards in the deck
// 72 Standard cards (1-9)
// 8 Skip Cards
// 8 Reverse Cards
// 8 Burn Cards
// 8 Reset Cards

/* How Shithead works:
 * Each player is delt 7 random cards from the deck, X facedown cards (variable), and X faceup cards on top of the facedown cards (same number of faceup)
 * Each player has a turn, starting with the person who lost last
 * Players may place as many of the same card as they want from their hand on the center pile
 * Players are only allowed to place a card on top of the pile if it's value is greater than, or equal to the top card of the center pile
 * If the top 4 cards of the center pile are the same, then the center pile gets burned (removed from play), and the player who burned it may take another turn
 * If the player is unable to place a card down on the center pile, they must pick up all the cards in the center pile
 * Once a player has run out of cards in their hand, they must use the faceup cards on their table on thier turn
 * Once the faceup cards have been played, the player must then choose a ranfom facedown card to play on each of their turns
 * Last person to empty their hand AND the cards on the table in front of them, loses and is considered the 'Shithead'
 * 
 * SKIP: Skips the turn of the next person
 * REVERSE: Reverses the turn order
 * BURN: Removes all the cards in the center pile from play, and allows the player to take another turn
 * RESET: Resets the center pile back to the lowest value (1)
 * Number 1-9: Standard cards that can be played
 */

[CreateAssetMenu(menuName = "Shithead Card Game", fileName = "New Shithead Card Game")]
public class ShitheadCardGame : ScriptableObject
{
    public ShitheadCardGame CreateInstance()
    {
        ShitheadCardGame newInstance = CreateInstance<ShitheadCardGame>();
        newInstance.maxPlayers = maxPlayers;
        newInstance.minPlayers = minPlayers;
        newInstance.skip = skip;
        newInstance.reverse = reverse;
        newInstance.burn = burn;
        newInstance.reset = reset;
        newInstance.standard = standard;
        return newInstance;
    }

    [Header("References")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Information")]
    [SerializeField] private string uniqueGameId;
    [SerializeField] private int maxPlayers = 12;
    [SerializeField] private int minPlayers = 2;

    [Header("Cards Per Player")]
    [SerializeField] private int skip = 1;
    [SerializeField] private int reverse = 1;
    [SerializeField] private int burn = 1;
    [SerializeField] private int reset = 1;
    [SerializeField] private int standard = 1;

    public int MaxPlayers => maxPlayers;
    public int MinPlayers => minPlayers;

    private Deck deck;
    private Deck centerPile;

    public void Setup(int playerCount)
    {
        SetupDeck(playerCount);
        SetupCenterPile();
    }

    private void SetupDeck(int playerCount)
    {
        if (deck != null)
            deck.SetupDeck();
        else
            deck = new Deck(uniqueGameId);

        for (int i = 0; i < playerCount; i++)
        {
            for (int j = 1; j <= standard * 9; j++)
            {
                deck.AddCard(new Card((ECardType)j));
            }
            for (int j = 0; j < skip; j++)
            {
                deck.AddCard(new Card(ECardType.SKIP));
            }
            for (int j = 0; j < reverse; j++)
            {
                deck.AddCard(new Card(ECardType.REVERSE));
            }
            for (int j = 0; j < burn; j++)
            {
                deck.AddCard(new Card(ECardType.BURN));
            }
            for (int j = 0; j < reset; j++)
            {
                deck.AddCard(new Card(ECardType.RESET));
            }
        }
    }

    private void SetupCenterPile()
    {
        if (centerPile != null)
            centerPile.ClearDeck();
        else
            centerPile = new Deck($"{uniqueGameId}_CenterPile");

        centerPile.AddCard(deck.PullCard());
    }
}
