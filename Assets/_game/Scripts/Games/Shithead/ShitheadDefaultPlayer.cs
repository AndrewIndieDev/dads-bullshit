using UnityEngine;

public class ShitheadDefaultPlayer : MonoBehaviour
{
    public Deck GetHand => hand;
    public Deck GetFacedownCards => facedownCards;
    public Deck GetFaceupCards => faceupCards;

    private Deck hand;
    private Deck facedownCards;
    private Deck faceupCards;

    public void Init(PersistentClient client)
    {
        hand = new Deck($"Player_{client.OwnerClientId}_Hand");
        facedownCards = new Deck($"Player_{client.OwnerClientId}_FaceDownCards");
        faceupCards = new Deck($"Player_{client.OwnerClientId}_FaceUpCards");
    }
}
