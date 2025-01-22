using Unity.Cinemachine;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _camera;

    public void SetAsMainCamera()
    {
        _camera.Priority = 10;
    }

    public void SetAsSubCamera()
    {
        _camera.Priority = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SetAsMainCamera();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            SetAsSubCamera();
        }
    }
}
