using AndrewDowsett.Networking;
using UnityEngine;

public class ColorChoiceButton : MonoBehaviour
{
    [SerializeField] private int siblingIndex;

    private void Start()
    {
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            if (transform.parent.GetChild(i).gameObject == gameObject)
            {
                siblingIndex = i;
                break;
            }
        }
    }

    public void ButtonClicked_ChooseColor()
    {
        RPCManager.Instance.TrySetSeatColorServerRPC(PersistentClient.LocalClient.OwnerClientId, siblingIndex);
    }
}
