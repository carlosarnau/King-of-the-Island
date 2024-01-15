using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;
//using Newtonsoft.Json;
using static Server;

public class Client : MonoBehaviour
{
    public string serverIP = "127.0.0.1"; // Replace with the server's IP address.
    public int serverPort = 12345; // Specify the port the server is listening on.
    public string username = "Unknown";
    public UdpClient udpClient;
    public IPEndPoint serverEndPoint;
    public GameObject clientPlayer;
    public Packet lastRepPacket;
    public List<Player> players = new List<Player>();

    private void Awake()
    {
        if ((PlayerPrefs.GetInt("isServer")) != 0)
            this.gameObject.SetActive(false);

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
            string message = "Hello from the client!";
            //byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            Packet pack = new Packet(username, Status.Connect, new Vector3(0, 1, 0), message);


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
        StartCoroutine(SendPacket(30.0f));
        StartCoroutine(WaitForMessages());
        //StartCoroutine(ProcessReplication(30.0f));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) 
        {
            string message = "Hello from the client!";
            //byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            Packet pack = new Packet(username, Status.Connect, new Vector3(0, 1, 0), message);


            byte[] messageBytes = SerializePacket(pack);  //Encoding.UTF8.GetBytes(responseMessage);

            udpClient.Send(messageBytes, messageBytes.Length, serverEndPoint);
        }

        //PROCESS REPLICATION
        if (lastRepPacket != null)
        {
            if (players.Count != lastRepPacket.playerList.Count)
            {
                Debug.Log("Client detected a Player has connected");
                players.Clear();
                foreach (Player player in lastRepPacket.playerList)
                {
                    players.Add(player);

                }
            }
            //TODO foreach Object add object to objectsList
            foreach (Player player in lastRepPacket.playerList)
            {

            }

            

            lastRepPacket = null;
            Debug.Log("Client processed a packet successfully");
        }

        //if (Input.GetKeyDown(KeyCode.C))
        //{

        //    Packet pack = new Packet("Manolo", Status.Chat, GameObject.Find("Player").transform.position, "I have chatted");
        //    byte[] messageBytes = SerializePacket(pack);  //Encoding.UTF8.GetBytes(responseMessage);

        //    udpClient.Send(messageBytes, messageBytes.Length, serverEndPoint);
        //}
    }

    private IEnumerator WaitForMessages()
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
            yield return null;
        }
    }

    //private IEnumerator ProcessReplication(float interval)
    //{
    //    while (true)
    //    {
    //        if (lastRepPacket != null)
    //        {
    //            if (players.Count != lastRepPacket.playerList.Count)
    //                Debug.Log("Client detected a Player has connected or disconnected");
    //            //TODO foreach Object add object to objectsList


    //            //foreach (Player player in playersOnline)
    //            //{
    //            //    udpListener.Send(messageBytes, messageBytes.Length, player.ip);

    //            //}
                
    //            lastRepPacket = null;
    //            Debug.Log("Client processed a packet successfully");
    //        }

    //        yield return new WaitForSeconds(1.0f / interval);
    //    }
    //}

    private IEnumerator SendPacket(float interval)
    {
        while (true)
        {
            Packet pack = new Packet(username, Status.Movement, clientPlayer.transform.position, "I have moved");
            byte[] messageBytes = SerializePacket(pack);  //Encoding.UTF8.GetBytes(responseMessage);
            udpClient.Send(messageBytes, messageBytes.Length, serverEndPoint);
            yield return new WaitForSeconds(1.0f / interval);
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, serverPort);
            byte[] receivedBytes = udpClient.EndReceive(ar, ref serverEndPoint);
            //string responseMessage = Encoding.UTF8.GetString(receivedBytes);
            Packet responsePacket = DeserializePacket(receivedBytes);
            // Handle the received response from the server.
            HandleResponse(responsePacket);
            //string receivedString = Encoding.ASCII.GetString(receivedBytes);
                //RepPacket repPacket = JsonConvert.DeserializeObject<RepPacket>(receivedString);
                //if (repPacket.status == Status.Replication)
            lastRepPacket = responsePacket;
            // Continue listening for more responses.
            //udpClient.BeginReceive(ReceiveCallback, null);
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
            Debug.Log("Received response from server: " + responsePacket.message);
        //if (responsePacket.status != Status.Replication)
            //Debug.Log("Received Replication: " + responsePacket.message);
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


}
