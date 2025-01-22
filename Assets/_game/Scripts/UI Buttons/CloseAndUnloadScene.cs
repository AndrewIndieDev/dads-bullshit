using AndrewDowsett.SceneLoading;
using UnityEngine;

public class CloseAndUnloadScene : MonoBehaviour
{
    public void ButtonClicked_Close()
    {
        SceneLoader.Instance.UnloadScene(gameObject.scene.name);
    }
}
