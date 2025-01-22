using System.Collections.Generic;
using UnityEngine;



public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private List<GameObject> ui_screens;

    [Header("Debug")]
    [SerializeField] private bool debug;

    public void SetScreen(EGameState screen)
    {
        int previousIndex = 0;
        for (int i = 0; i < ui_screens.Count; i++)
        {
            if (ui_screens[i].activeInHierarchy)
                previousIndex = i;
            ui_screens[i].SetActive(i == (int)screen);
        }
        if (debug)
            DebugMessage($"Game state has been updated from {(EGameState)previousIndex} to {screen}!");
    }

    public void SetScreen(int index) => SetScreen((EGameState)index);

    private void DebugMessage(string message)
    {
        if (!debug)
            return;
        Debug.Log($"[UIManager] :: {message}");
    }
}
