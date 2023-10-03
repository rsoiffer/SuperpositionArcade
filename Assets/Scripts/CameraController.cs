using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        _camera.orthographicSize = Screen.height / 2f;
        transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
    }
}