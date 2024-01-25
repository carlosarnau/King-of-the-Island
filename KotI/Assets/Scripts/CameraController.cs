using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    void Start()
    {
        gameObject.GetComponent<CinemachineFreeLook>().LookAt = GameObject.Find("Player").transform;
        gameObject.GetComponent<CinemachineFreeLook>().Follow = GameObject.Find("Player").transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
