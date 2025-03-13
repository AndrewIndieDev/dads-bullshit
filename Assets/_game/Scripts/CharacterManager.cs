using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public Character[] Characters => characters;

    [SerializeField] private Character[] characters;

    public void SetCharacter(int index, PersistentClient client) => characters[index].SetClient(client);
}
