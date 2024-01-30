using UnityEngine;
using Cinemachine;

public class GameManager : MonoBehaviour
{
    public float upwardVelocity = 5f;
    private GameObject waterObject;
    public bool startGame = false;

    public CinemachineFreeLook cameraController;

    private bool isWaterCollisionActive = true;

    private void Start()
    {
        cameraController = GameObject.Find("Third Person Camera").GetComponent<CinemachineFreeLook>();
        // Find the water game object by name
        waterObject = GameObject.Find("Water");

        if (waterObject == null)
        {
            Debug.LogError("Water game object not found. Make sure the object is named 'Water'.");
        }
    }

    private void Update()
    {
        // Move the water object upward in the Y-axis
        if (waterObject != null && startGame)
        {
            MoveWaterUp();
            CheckPlayerCollision();
        }
    }

    private void MoveWaterUp()
    {
        // Move the water object upward in the Y-axis
        waterObject.transform.Translate(Vector3.up * upwardVelocity * Time.deltaTime);
    }

    private void CheckPlayerCollision()
    {
        // Assuming your player has a collider and is tagged as "Player"
        Collider playerCollider = GameObject.FindWithTag("Player").GetComponent<CapsuleCollider>();

        if (playerCollider != null && isWaterCollisionActive && playerCollider.bounds.Intersects(waterObject.GetComponent<Collider>().bounds))
        {
            GameObject.Find("Client").GetComponent<Client>().SendDiePacket();
            cameraController.LookAt = GameObject.Find(GameObject.Find("Client").GetComponent<Client>().playersObjects[0].name).transform;
            cameraController.Follow = GameObject.Find(GameObject.Find("Client").GetComponent<Client>().playersObjects[0].name).transform;
            KillPlayer();
            startGame = false;
        }
    }

    private void KillPlayer()
    {
        Debug.Log("Player killed by water!");

        // Destroy the player GameObject
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            Destroy(playerObject);
        }
        else
        {
            Debug.LogError("Player game object not found. Make sure the object is tagged as 'Player'.");
        }
    }

    public Vector3 RequestSpawn()
    {
        Vector2 spawnPosition;
        RaycastHit hit;

        spawnPosition.x = Random.Range(-60, 150);
        spawnPosition.y = Random.Range(-60, 150);

        Vector3 direction = new Vector3(35, 5, 50) - new Vector3(spawnPosition.x, 5, spawnPosition.y);

        Debug.DrawLine(new Vector3(spawnPosition.x, 5, spawnPosition.y), direction * 100f);

        if (Physics.Raycast(new Vector3(spawnPosition.x, 5, spawnPosition.y), direction, out hit, 100f))
        {
            //cube.transform.position = hit.point;
            return hit.point;
        }

        return RequestSpawn();
    }
}