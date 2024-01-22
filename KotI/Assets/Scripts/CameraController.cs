using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    
    void Start()
    {
        gameObject.GetComponent<CinemachineFreeLook>().LookAt = GameObject.Find("Player").transform;
        gameObject.GetComponent<CinemachineFreeLook>().Follow = GameObject.Find("Player").transform;
    }
}
