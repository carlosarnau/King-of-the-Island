using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System.Linq;
//using Newtonsoft.Json;
using static Server;
using Random = UnityEngine.Random;

public class Client : MonoBehaviour
{
    public string serverIP = "127.0.0.1"; // Replace with the server's IP address.
    public int serverPort = 12345; // Specify the port the server is listening on.
    public string username = "Unknown";
    public UdpClient udpClient;
    public IPEndPoint serverEndPoint;
    public GameObject clientPrefab;
    public GameObject playerPrefab;
    public GameObject clientPlayer;
    public Packet lastRepPacket;
    public List<Player> players = new List<Player>();
    public List<GameObject> playersObjects = new List<GameObject>();
    private Vector3 predictedPosition;
    public float reconciliationThreshold = 0.5f; // Adjust as needed
    public float interpolationTime = 0.5f; // Adjust as needed
    public float moveSpeed = 5.0f; // Adjust as needed

    private void Awake()
    {
        if ((PlayerPrefs.GetInt("isServer")) != 0)
        {
            this.gameObject.SetActive(false);
        }
        else
        {
            clientPlayer = Instantiate(clientPrefab, Vector3.zero, Quaternion.identity);
        }

        if (PlayerPrefs.GetString("ipAddress") != null)
            serverIP = PlayerPrefs.GetString("ipAddress");

        if (PlayerPrefs.GetString("username") != null)
            username = PlayerPrefs.GetString("username");
    }

    private void Start()
    {
        try
        {
            // Create a UDP client.
            udpClient = new UdpClient();

            // Connect to the server with the provided IP and port.
            serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

            // Send a message to the server.
            Vector3 col = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            string message = JsonUtility.ToJson(col);
            //byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            Packet pack = new Packet(username, Status.Connect, new Vector3(0, 1, 0), Quaternion.identity, message);

            byte[] messageBytes = SerializePacket(pack);  //Encoding.UTF8.GetBytes(responseMessage);

            udpClient.Send(messageBytes, messageBytes.Length, serverEndPoint);

            clientPlayer.GetComponentInChildren<Renderer>().material.color = new Color(col.x, col.y, col.z);

            Debug.Log("UDP client connected to " + serverIP + ":" + serverPort);

            // Start listening for responses asynchronously.
            udpClient.BeginReceive(ReceiveCallback, null);
        }
        catch (SocketException e)
        {
            Debug.LogError("Error setting up UDP client: " + e.Message);
        }
        StartCoroutine(SendPacket(30.0f));
        StartCoroutine(WaitForMessages(60.0f));
        //StartCoroutine(ProcessReplication(30.0f));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            string message = "Hello from the client!";
            //byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            Packet pack = new Packet(username, Status.Connect, new Vector3(0, 1, 0), Quaternion.identity, message);

            byte[] messageBytes = SerializePacket(pack);  //Encoding.UTF8.GetBytes(responseMessage);

            udpClient.Send(messageBytes, messageBytes.Length, serverEndPoint);

            predictedPosition += clientPlayer.transform.forward * Time.deltaTime * moveSpeed;
            clientPlayer.transform.position = predictedPosition;
        }

        //PROCESS REPLICATION
        if (lastRepPacket != null)
        {
            //CONNECT A PLAYER
            if (lastRepPacket.playerList.Count > players.Count + 1)
            {
                for (int i = 0; i < lastRepPacket.playerList.Count; i++)
                {
                    if (lastRepPacket.playerList[i].userID != username && !players.Exists(player => player.userID == lastRepPacket.playerList[i].userID))
                    {
                        AddNewPlayer(lastRepPacket.playerList[i]);
                        Debug.Log("añadido el player " + lastRepPacket.playerList[i].userID);
                    }
                }
            }

            if (players.Count > 0 && players.Count == lastRepPacket.playerList.Count - 1)
            {
                int index = 0;

                for (int i = 0; i < lastRepPacket.playerList.Count; i++)
                {
                    if (lastRepPacket.playerList[i].userID != username)
                    {
                        players[i-index].position = lastRepPacket.playerList[i].position;
                        playersObjects[i-index].transform.position = lastRepPacket.playerList[i].position;
                        playersObjects[i-index].transform.rotation = lastRepPacket.playerList[i].rotation;
                    }
                    else 
                    {
                        index = 1;
                    }
                }
            }

            if (lastRepPacket.playerList.Count > 0 && lastRepPacket.playerList.Count < players.Count + 1)
            {
                int playerIndex = 0;
                for (int i = 0; i < players.Count; i++)
                {
                    if (!lastRepPacket.playerList.Exists(player => player.userID == players[i].userID))
                    {
                        playerIndex = i;
                    }
                }
                Debug.Log(playerIndex);
                players.RemoveAt(playerIndex);
                Destroy(playersObjects[playerIndex]);
                playersObjects.RemoveAt(playerIndex);
            }
            lastRepPacket = null;
            //Debug.Log("Client processed a packet successfully");
        }
    }

    private IEnumerator WaitForMessages(float interval)
    {
        while (true)
        {
            try
            {
                // Start listening for incoming messages asynchronously.
                udpClient.BeginReceive(ReceiveCallback, null);
            }
            catch (SocketException e)
            {
                Debug.LogError("Error waiting for message UDP server: " + e.Message);
            }
            yield return new WaitForSeconds(1.0f / interval);
        }
    }

    private IEnumerator ProcessReplication(float interval)
    {
        while (true)
        {
            if (lastRepPacket != null)
            {
                if (players.Count != lastRepPacket.playerList.Count)
                    Debug.Log("Client detected a Player has connected or disconnected");
                //TODO foreach Object add object to objectsList

                //foreach (Player player in playersOnline)
                //{
                //    udpListener.Send(messageBytes, messageBytes.Length, player.ip);

                //}

                lastRepPacket = null;
                Debug.Log("Client processed a packet successfully");
            }
            yield return new WaitForSeconds(1.0f / interval);
        }
    }

    private IEnumerator SendPacket(float interval)
    {
        while (true)
        {
            // Adding a ranadom delay between 0 and 0.1 seconds to simulate Jitter
            yield return new WaitForSeconds(Random.Range(0, 0.1f));

            // Simulate the packet loss by randomly deciding whether to send the packet
            if (Random.value < 0.5f) // Adjustable (in this case 0.9 represents a 90% chance of sending the package)
            { 
            Packet pack = new Packet(username, Status.Movement, clientPlayer.transform.position, clientPlayer.transform.rotation, "I have moved");
            byte[] messageBytes = SerializePacket(pack);  //Encoding.UTF8.GetBytes(responseMessage);
            udpClient.Send(messageBytes, messageBytes.Length, serverEndPoint);
            }
            yield return new WaitForSeconds(1.0f / interval);
        }
    }

    private IEnumerator InterpolatePosition(Vector3 targetPosition)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = predictedPosition;

        while (elapsedTime < interpolationTime)
        {
            clientPlayer.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / interpolationTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Set the final position
        clientPlayer.transform.position = targetPosition;
        predictedPosition = targetPosition;
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, serverPort);
            byte[] receivedBytes = udpClient.EndReceive(ar, ref serverEndPoint);

            // Simulate the packet loss by randomly deciding whether to process the received packet
            if (Random.value < 0.5f) // Adjustable(in this case 0.9 represents a 90 % chance of sending the package)
            {
                Packet responsePacket = DeserializePacket(receivedBytes);
                HandleResponse(responsePacket);
                lastRepPacket = responsePacket;

                //string responseMessage = Encoding.UTF8.GetString(receivedBytes);
                // Handle the received response from the server.
                //string receivedString = Encoding.ASCII.GetString(receivedBytes);
                //RepPacket repPacket = JsonConvert.DeserializeObject<RepPacket>(receivedString);
                //if (repPacket.status == Status.Replication)
                // Continue listening for more responses.
                //udpClient.BeginReceive(ReceiveCallback, null);
            }
        }
        catch (SocketException e)
        {
            Debug.LogError("UDP receive error: " + e.Message);
        }
    }

    private void HandleResponse(Packet responsePacket)
    {
        // Handle the received response from the server.
        if (responsePacket.status != Status.Idle)
            //Debug.Log("Received response from server: " + responsePacket.message);
            if (responsePacket.status != Status.Replication)
            {
                //Debug.Log("Received Replication: " + responsePacket.message);
            }

        if (responsePacket.status == Status.Movement)
        {
            Vector3 serverPosition = responsePacket.position;
            // players[i].playerState = responsePacket.playerState;

            // Perform reconciliation
            float distance = Vector3.Distance(predictedPosition, serverPosition);

            if (distance > reconciliationThreshold)
            {
                // Adjust the local player's position to match the server's position
                StartCoroutine(InterpolatePosition(serverPosition));
            }
        }
    }

    private void OnDestroy()
    {
        if (udpClient != null)
        {
            udpClient.Close();
        }
    }

    public void setIpDestination(string newIp)
    {
        serverIP = newIp;
    }

    public byte[] SerializePacket(Packet toSend)
    {
        string json = JsonUtility.ToJson(toSend);
        return System.Text.Encoding.UTF8.GetBytes(json);
    }
    public Packet DeserializePacket(byte[] toReceive)
    {
        string json = System.Text.Encoding.UTF8.GetString(toReceive);
        return JsonUtility.FromJson<Packet>(json);
    }

    public void OnApplicationQuit()
    {
        string message = "Bye from the client!";
        Packet pack = new Packet(username, Status.Disconnect, new Vector3(0, 1, 0), Quaternion.identity, message);
        byte[] messageBytes = SerializePacket(pack);  //Encoding.UTF8.GetBytes(responseMessage);
        udpClient.Send(messageBytes, messageBytes.Length, serverEndPoint);
        //byte[] messageBytes = Encoding.UTF8.GetBytes(message);
    }

    public void AddNewPlayer(Player player)
    {
        players.Add(player);
        GameObject temp = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        temp.name = "Player " + player.userID;
        playersObjects.Add(temp);
    }
}