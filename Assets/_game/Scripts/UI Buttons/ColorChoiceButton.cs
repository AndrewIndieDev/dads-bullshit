using AndrewDowsett.Networking;
using UnityEngine;

public class ColorChoiceButton : MonoBehaviour
{
    [SerializeField] private int siblingIndex;

    private void Start()
    {
        siblingIndex = transform.GetSiblingIndex();
    }

    public void ButtonClicked_ChooseColor()
    {
        RPCManager.Instance.TrySetSeatColorServerRPC(PersistentClient.LocalClient.OwnerClientId, siblingIndex);
    }
}
