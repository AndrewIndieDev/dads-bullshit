using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public List<Transform> spawnPoints = new();
    public List<Character> characters = new();

    public void SpawnPlayer(PersistentClient client)
    {

    }
}
