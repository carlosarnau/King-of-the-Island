using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
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
    public GameObject serverCamera;
    public GameObject clientCamera;
    public List<Packet> lastRepPackets = new List<Packet>();
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
            //serverCamera.SetActive(false);
        }
        else
        {
            clientPlayer = Instantiate(clientPrefab, GameObject.Find("GM").GetComponent<GameManager>().RequestSpawn(), Quaternion.identity);
            clientPlayer.name = "Player";
            //clientCamera.SetActive(false);
        }

        if (PlayerPrefs.GetString("ipAddress") != null)
            serverIP = PlayerPrefs.GetString("ipAddress");

        if (PlayerPrefs.GetString("username") != null)
            username = PlayerPrefs.GetString("username");

        GameObject.Find("Player Mesh").GetComponent<SkinnedMeshRenderer>().materials[1].SetColor("_Color", new Color(PlayerPrefs.GetFloat("ColorR"), PlayerPrefs.GetFloat("ColorG"), PlayerPrefs.GetFloat("ColorB")));
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
            Vector3 col = new Vector3(PlayerPrefs.GetFloat("ColorR"), PlayerPrefs.GetFloat("ColorG"), PlayerPrefs.GetFloat("ColorB"));
            string message = JsonUtility.ToJson(col);
            //byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            Packet pack = new Packet(username, Status.Connect, clientPlayer.transform.position, new Vector3(0, 0, 0), new Vector3(0, 0, 0), Quaternion.identity, /*PlayerState.Idle,*/ message);

            byte[] messageBytes = SerializePacket(pack);  //Encoding.UTF8.GetBytes(responseMessage);

            udpClient.Send(messageBytes, messageBytes.Length, serverEndPoint);

            Debug.Log("UDP client connected to " + serverIP + ":" + serverPort);

            // Start listening for responses asynchronously.
            udpClient.BeginReceive(ReceiveCallback, null);
        }
        catch (SocketException e)
        {
            Debug.LogError("Error setting up UDP client: " + e.Message);
        }
        StartCoroutine(SendPacket(64.0f));
        StartCoroutine(WaitForMessages(240.0f));
        //StartCoroutine(ProcessReplication(30.0f));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            string message = "Hello from the client!";
            //byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            Packet pack = new Packet(username, Status.Connect, new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), Quaternion.identity, /*PlayerState.Idle,*/ message);

            byte[] messageBytes = SerializePacket(pack);  //Encoding.UTF8.GetBytes(responseMessage);

            udpClient.Send(messageBytes, messageBytes.Length, serverEndPoint);

            predictedPosition += clientPlayer.transform.forward * Time.deltaTime * moveSpeed;
            clientPlayer.transform.position = predictedPosition;
        }
        //okay now that everything is replicated, let's make enemies move and stuff (ignore the beautiful functions of interpolation down below, they're useless, they didn't work anyways)
        for (int i = 0; i < playersObjects.Count; i++)
        {
            if (players[i].vel.sqrMagnitude > 0.001)
                //layersObjects[i].transform.SetPositionAndRotation(playersObjects[i].transform.position + 2.0f * Time.deltaTime * players[i].vel, playersObjects[i].transform.rotation);
                playersObjects[i].transform.SetPositionAndRotation(playersObjects[i].transform.position + 2.0f * Time.deltaTime * (players[i].vel + new Vector3(0, 2, 0)), playersObjects[i].transform.rotation);
        }


        List<Packet> tempPackets = new(lastRepPackets);
        bool hasFound = false;
        lastRepPackets.Clear();


        //PROCESS REPLICATION
        foreach (Packet lastRepPacket in tempPackets)
        {
            if (lastRepPacket != null)
            {
                //Debug.Log(JsonUtility.ToJson(lastRepPacket));

                if (lastRepPacket.status == Status.Bounce && hasFound == false)
                {
                    Debug.Log(JsonUtility.ToJson(lastRepPacket));

                    if (lastRepPacket.message == username)
                    {
                        if (lastRepPacket.message == username)
                        {
                            GameObject.Find("Player").GetComponent<CharacterMovement>().BounceBack(lastRepPacket.position.x, lastRepPacket.position.y, lastRepPacket.position.z);
                            Debug.Log(lastRepPacket.position);

                            hasFound = true;
                        }
                    }
                }

                if (lastRepPacket.status == Status.StartGame)
                {
                    GameObject.Find("GM").GetComponent<GameManager>().startGame = true;
                }

                if (lastRepPacket.status == Status.Die && lastRepPacket.message != username)
                {
                    GameObject.Find("GM").GetComponent<GameManager>().startGame = false;
                    GameObject.Find("Player").GetComponent<CharacterMovement>().UpdateWin();
                    GameObject.Find("Staff").SetActive(false);

                    for (int i = 0; i < playersObjects.Count; i++)
                    {
                        if (playersObjects[i] != null && playersObjects[i].name == lastRepPacket.message)
                        {
                            Destroy(playersObjects[i]);
                            playersObjects.RemoveAt(i);
                        }
                    }
                }

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
                            if (lastRepPacket.playerList[i].position != players[i - index].position)
                            {
                                players[i - index].position = lastRepPacket.playerList[i].position;
                                playersObjects[i - index].transform.position = lastRepPacket.playerList[i].position;
                                playersObjects[i - index].transform.rotation = lastRepPacket.playerList[i].rotation;
                            }
                            if (lastRepPacket.playerList[i].vel != players[i - index].vel)
                            players[i - index].vel = lastRepPacket.playerList[i].vel;
                            
                            players[i - index].state = lastRepPacket.playerList[i].state;
                            //playersObjects[i - index].GetComponent<Rigidbody>().velocity = lastRepPacket.playerList[i].vel;

                            EnemyController.EnemyState enemyState;
                            if (Enum.TryParse(players[i - index].state.ToString(), out enemyState))
                            {
                                if(playersObjects[i - index] != null)
                                {
                                    playersObjects[i - index].GetComponent<EnemyController>().state = enemyState;
                                }
                                //playersObjects[i - index].GetComponent<EnemyController>().dir = lastRepPacket.playerList[i].dir;
                                //playersObjects[i - index].GetComponent<EnemyController>().moveDirection = lastRepPacket.playerList[i].vel;
                            }
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
                //lastRepPacket = null;
                //Debug.Log("Client processed a packet successfully");
            }
        }
        tempPackets.Clear();

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

    private IEnumerator SendPacket(float interval)
    {
        while (true)
        {
            // Adding a ranadom delay between 0 and 0.1 seconds to simulate Jitter
            yield return new WaitForSeconds(Random.Range(0, 0.1f));
          
            Packet pack = new Packet(username, Status.Movement, clientPlayer.transform.position, clientPlayer.GetComponent<CharacterMovement>().dir, clientPlayer.GetComponent<CharacterController>().velocity, clientPlayer.transform.rotation, clientPlayer.GetComponent<CharacterMovement>().playerState.ToString());
            byte[] messageBytes = SerializePacket(pack);  //Encoding.UTF8.GetBytes(responseMessage);
            udpClient.Send(messageBytes, messageBytes.Length, serverEndPoint);
            
            yield return new WaitForSeconds(1.0f / interval);
        }
    }

    public bool SendBouncePacket(string name, float x, float z, float force)
    {
        Vector3 vector3 = new Vector3(x, z, force);
        Packet pack = new Packet(username, Status.Bounce, vector3, clientPlayer.GetComponent<CharacterMovement>().dir, clientPlayer.GetComponent<CharacterMovement>().moveDirection, clientPlayer.transform.rotation, name);
        byte[] messageBytes = SerializePacket(pack);  //Encoding.UTF8.GetBytes(responseMessage);
        udpClient.Send(messageBytes, messageBytes.Length, serverEndPoint);

        //Debug.Log(username + " ha pegado a " + name);

        //Debug.Log(JsonUtility.ToJson(pack));

        return true;
    }

    public void SendDiePacket()
    {
        Packet pack = new Packet(username, Status.Die, Vector3.forward, clientPlayer.GetComponent<CharacterMovement>().dir, clientPlayer.GetComponent<CharacterMovement>().moveDirection, clientPlayer.transform.rotation, name);
        byte[] messageBytes = SerializePacket(pack);  //Encoding.UTF8.GetBytes(responseMessage);
        udpClient.Send(messageBytes, messageBytes.Length, serverEndPoint);
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

            // Simulate the packet loss by randomly deciding whether to process the received packet
            //if (Random.value < 0.5f) // Adjustable(in this case 0.9 represents a 90 % chance of sending the package)
            {
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, serverPort);
                byte[] receivedBytes = udpClient.EndReceive(ar, ref serverEndPoint);
                Packet responsePacket = DeserializePacket(receivedBytes);
                HandleResponse(responsePacket);
                lastRepPackets.Add(responsePacket);
                //lastRepPacket = responsePacket;

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

        //if (responsePacket.status == Status.Movement) cringe code
        //{
        //    Vector3 serverPosition = responsePacket.position;
        //    // players[i].playerState = responsePacket.playerState;

        //    // Perform reconciliation
        //    float distance = Vector3.Distance(predictedPosition, serverPosition);

        //    if (distance > reconciliationThreshold)
        //    {
        //        // Adjust the local player's position to match the server's position
        //        StartCoroutine(InterpolatePosition(serverPosition));
        //    }
        //}
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
        Packet pack = new Packet(username, Status.Disconnect, new Vector3(0, 1, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0), Quaternion.identity, message);
        byte[] messageBytes = SerializePacket(pack);  //Encoding.UTF8.GetBytes(responseMessage);
        udpClient.Send(messageBytes, messageBytes.Length, serverEndPoint);
        //byte[] messageBytes = Encoding.UTF8.GetBytes(message);
    }

    public void AddNewPlayer(Player player)
    {
        players.Add(player);
        GameObject temp = Instantiate(playerPrefab, player.position, Quaternion.identity);
        temp.name = player.userID;
        temp.GetComponentInChildren<SkinnedMeshRenderer>().materials[1].SetColor("_Color", new Color(player.color.r, player.color.g, player.color.b, player.color.a));
        playersObjects.Add(temp);
    }
}