using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public float rotationSpeed;

    private void Start()
    {
        rb = GameObject.Find("Client Player").GetComponent<Rigidbody>();
        orientation = GameObject.Find("Orientation").GetComponent<Transform>();
        player = GameObject.Find("Client Player").GetComponent<Transform>();
        playerObj = GameObject.Find("Ninja").GetComponent<Transform>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 viewDir = player.position - new Vector3(transform.position.x, transform.position.y, transform.position.z);
        orientation.forward = viewDir;

        Debug.Log(orientation.forward);

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;


        if (inputDir != Vector3.zero)
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);

        player.transform.rotation = new Quaternion(player.transform.rotation.x, orientation.transform.rotation.y, player.transform.rotation.z, player.transform.rotation.w);

    }
}
