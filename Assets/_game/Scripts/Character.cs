using AndrewDowsett.ObjectUIBinding;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private List<GameObject> _characterVisuals;
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private ObjectBoundToUI _bountToUI;
    [SerializeField] private Color _playerColor;

    public bool IsAssigned => _client != null;

    private PersistentClient _client;

    private void Start()
    {
        _bountToUI.Show(_playerColor);
    }

    public void SetClient(PersistentClient client)
    {
        _client = client;
        _characterVisuals.ForEach(x => x.SetActive(_client.CharacterIndex == _characterVisuals.IndexOf(x)));

        if (client.IsOwner)
        {
            SetAsMainCamera();
        }

        _bountToUI.Hide();
    }

    public void SetAsMainCamera()
    {
        _camera.Priority = 10;
    }

    public void ResetCamera()
    {
        _camera.Priority = 0;
    }

    public void ResetCharacter()
    {
        ResetCamera();
        _client = null;
        _bountToUI.Show(_playerColor);
    }
}
