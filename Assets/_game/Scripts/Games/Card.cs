public enum ECardType
{
    ONE = 1,
    TWO,
    THREE,
    FOUR,
    FIVE,
    SIX,
    SEVEN,
    EIGHT,
    NINE,
    SKIP,
    REVERSE,
    BURN,
    RESET
}

public class Card
{
    public ECardType CardType { get; }
    public int CardValue { get { return (int)CardType; } }

    public Card(ECardType cardType)
    {
        CardType = cardType;
    }
}
