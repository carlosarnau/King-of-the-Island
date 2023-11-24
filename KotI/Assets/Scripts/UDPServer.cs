using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;


public class UDPServer : MonoBehaviour
{
    // Start is called before the first frame update
    public int port = 12345; // Specify the port to listen on.

    public UdpClient udpListener;
    public GameObject playerPrefab;
    public class Player
    {
        public string userID;
        public GameObject userGO;
        public Vector3 position;
        public Player(string userID, GameObject userGO, Vector3 position)
        {
            this.userID = userID;
            this.userGO = userGO;
            this.position = position;
        }
    }

    public List<Player> playersOnline = new List<Player>();
    public GameObject playerFromClient;
    public Packet lastPacket;

    private void Awake()
    {
        if ((PlayerPrefs.GetInt("isServer")) != 1)
            this.gameObject.SetActive(false);
    }

    private void Start()
    {
        playerFromClient = GameObject.Find("PlayerFromClient");
        try
        {
            // Create a UDP listener on the specified port.
            udpListener = new UdpClient(port);
            Debug.Log("UDP server is listening on port " + port);

            // Start listening for incoming messages asynchronously.
            //udpListener.BeginReceive(ReceiveCallback, null);
        }
        catch (SocketException e)
        {
            Debug.LogError("Error setting up UDP server: " + e.Message);
        }
    }

    private void Update()
    {
        try
        {
            // Create a UDP listener on the specified port.

            // Start listening for incoming messages asynchronously.
            udpListener.BeginReceive(ReceiveCallback, null);
        }
        catch (SocketException e)
        {
            Debug.LogError("Error setting up UDP server: " + e.Message);
        }

        if (lastPacket!= null) 
        {
            if (lastPacket.status == Status.Connect)
            {
                Player newPlayer = new Player(lastPacket.user, GameObject.Find("PlayerFromClient"), new Vector3(0, 0, 0));
                playersOnline.Add(newPlayer);
            }
            //if (lastPacket.status == Status.Chat)
                //TODO when we received some chat hehe
            if (lastPacket.status == Status.Movement)
            {
                
                playerFromClient.transform.position = new Vector3(lastPacket.position.x, lastPacket.position.y+5, lastPacket.position.z);
                //TODO update Players List
            }
            lastPacket = null;
        }
        
    }

    public void ReceiveCallback(IAsyncResult ar)
    {

        try
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            byte[] receivedBytes = udpListener.EndReceive(ar, ref remoteEndPoint);
            //string message = Encoding.UTF8.GetString(receivedBytes);
            Packet message = DeserializePacket(receivedBytes);

            // Handle the received message, e.g., respond to the client.
            HandleMessage(message, remoteEndPoint);

            // Continue listening for more messages.
            //udpListener.BeginReceive(ReceiveCallback, null);
        }
        catch (SocketException e)
        {
            Debug.LogError("UDP receive error: " + e.Message);
        }


    }

    public void HandleMessage(Packet message, IPEndPoint remoteEndPoint)
    {
        // Handle the received message here. You can send a response back to the client if needed.
        Debug.Log("Received message from " + remoteEndPoint + ": " + message.message);

        lastPacket = message;

        string messageToSend = "";

        //if message.status is Connect means it's the first time
        if (message.status == Status.Connect)
        {
            messageToSend = "Welcome, " + message.user;
        }
        if (message.status == Status.Chat)
            messageToSend = message.user + " chatted: " + message.message;


        messageToSend = "Moving user " + message.user + " to " + message.position;
        // Example: Sending a response to the client.
        //string responseMessage = "Hello from the server!";

        Packet pack = new Packet("Server", Status.Connect, new Vector3(0, 0, 0), messageToSend);


        byte[] responseBytes = SerializePacket(pack);  //Encoding.UTF8.GetBytes(responseMessage);
        udpListener.Send(responseBytes, responseBytes.Length, remoteEndPoint);
    }

    private void OnDestroy()
    {
        if (udpListener != null)
        {
            udpListener.Close();
        }
    }


    public class Packet
    {
        public string user;
        public Status status;
        public Vector3 position;
        public string message;
        public Packet(String user, Status status, Vector3 position, string message)
        {
            this.user = user;
            this.status = status;
            this.position = position;
            this.message = message;
        }
    }
    public enum Status
    {
        Connect, 
        Disconnect, 
        Chat,
        Movement,
        Idle
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
