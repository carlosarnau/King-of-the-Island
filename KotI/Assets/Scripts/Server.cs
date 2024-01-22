using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
//using Newtonsoft.Json;

public class Server : MonoBehaviour
{
    // Start is called before the first frame update
    public int port = 12345; // Specify the port to listen on.

    public UdpClient udpListener;
    public GameObject playerPrefab;

    public List<Player> playersOnline = new List<Player>();

    public List<Packet> receivedPackets;

    public List<GameObject> playerObjects;

    [Serializable]
    public class Player
    {
        public string userID;
        public Vector3 position;
        public Vector3 vel;
        public Quaternion rotation;
        public int life;
        public IPEndPoint ip;
        //public float x, y, z;

        public Player(string userID_, GameObject userGO_, Vector3 position_, Vector3 vel_, Quaternion rotation_, int life_, IPEndPoint ip_)
        {
            userID = userID_;
            position = position_;
            vel = vel_;
            rotation = rotation_;
            life = life_;
            ip = ip_;
            //x = position_.x;
            //y = position_.y;
            //z = position_.z;
        }
    }
    [Serializable]
    public class Packet
    {
        public string user;
        public Status status;
        public Vector3 position;
        public Vector3 vel;
        public Quaternion rotation;
        public string message;
        public IPEndPoint ip;
        public List<Player> playerList;
        public List<Object> objectList;
        public DateTime time;

        public Packet(String user, Status status, Vector3 position, Vector3 vel,  Quaternion rotation_, string message)
        {
            this.user = user;
            this.status = status;
            this.position = position;
            this.rotation = rotation_;
            this.message = message;
            playerList = new List<Player>();
            objectList = new List<Object>();
            time = DateTime.UtcNow;
            this.vel = vel;
            //this.ip = ip;
        }
    }
    [Serializable]
    public class Object
    {
        public uint objectID { get; set; }
        public Vector3 position { get; set; }
        public ObjectType type { get; set; }

        public Object(uint objectID, Vector3 position, ObjectType type)
        {
            this.objectID = objectID;
            this.position = position;
            this.type = type;
        }
    }
    public enum ObjectType
    {
        Item,
        unknown
    }
    public enum Status
    {
        Idle,
        Connect,
        Replication,
        Disconnect,
        Chat,
        Movement
    }
    private void Awake()
    {
        if ((PlayerPrefs.GetInt("isServer")) != 1)
            this.gameObject.SetActive(false);
    }

    private void Start()
    {
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
        StartCoroutine(WaitForMessages());
        StartCoroutine(SendReplication(30.0f));
    }

    private IEnumerator WaitForMessages()
    {
        while (true)
        {
            try
            {
                // Start listening for incoming messages asynchronously.
                udpListener.BeginReceive(ReceiveCallback, null);
            }
            catch (SocketException e)
            {
                Debug.LogError("Error waiting for message UDP server: " + e.Message);
            }
            yield return null;
        }
    }

    private void Update()
    {
        /*try
        {
            // Create a UDP listener on the specified port.

            // Start listening for incoming messages asynchronously.
            udpListener.BeginReceive(ReceiveCallback, null);
        }
        catch (SocketException e)
        {
            Debug.LogError("Error setting up UDP server: " + e.Message);
        }
        */
        foreach (Packet lastPacket in receivedPackets)
        {
            if (lastPacket != null)
            {
                ProcessPacket(lastPacket);
            }
        }
        receivedPackets.Clear();
        //TODO update world
    }

    public IEnumerator SendReplication(float interval)
    {
        while (true)
        {
            if (playersOnline.Count > 0)
            {
                Packet pack = new Packet("Server", Status.Replication, new Vector3(0, 0, 0), new Vector3(0, 0, 0), Quaternion.identity, "Replication");
                foreach (Player player in playersOnline)
                {
                    pack.playerList.Add(player);
                }
                //string messageString = JsonConvert.SerializeObject(pack/*, new JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore }*/);  //Encoding.UTF8.GetBytes(responseMessage); <<  get cancer
                var messageBytes = SerializePacket(pack);

                foreach (Player player in playersOnline)
                {
                    udpListener.Send(messageBytes, messageBytes.Length, player.ip);
                }
                Debug.Log("Sent replication " + JsonUtility.ToJson(pack));
            }
            //TODO foreach Object add object to objectsList
            yield return new WaitForSeconds(1.0f / interval);
        }
    }

    public void ProcessPacket(Packet pack)
    {
        if (pack.status == Status.Connect)
        {
            Player newPlayer = new Player(pack.user, GameObject.Find("PlayerFromClient"), new Vector3(0, 0, 0), new Vector3(0, 0, 0), Quaternion.identity, 100, pack.ip);
            playersOnline.Add(newPlayer);
            if (playersOnline.Count > 0 && playerObjects.Count < playersOnline.Count)
            {
                GameObject temp = Instantiate(playerPrefab);
                Vector3 col = JsonUtility.FromJson<Vector3>(pack.message);
                temp.GetComponentInChildren<Renderer>().material.color = new Color(col.x, col.y, col.z);
                playerObjects.Add(temp);
                //playerObjects.Add(new GameObject)
            }
            Debug.Log("Player joined:" + newPlayer.userID + newPlayer.ip + pack.ip);
        }

        else if (pack.status == Status.Disconnect)
        {
            for (int i = 0; i < playersOnline.Count; i++)
            {
                if (playersOnline[i].userID == pack.user)
                {
                    Destroy(playerObjects[i]);
                    playerObjects.RemoveAt(i);
                    playersOnline.Remove(playersOnline[i]);
                }
                //if ()
            }
            Debug.Log("Player " + pack.user + " disconnected");
        }

        // Uncomment this block when you have the implementation for chat
        //if (lastPacket.status == Status.Chat)
        //{
        //    // TODO: Implement chat processing
        //}

        if (pack.status == Status.Movement)
        {
            for (int i = 0; i < playersOnline.Count; i++)
            {
                if (pack.user == playersOnline[i].userID && playerObjects[i] != null)
                {
                    //playerObjects[i].transform.position = new Vector3(pack.position.x, pack.position.y, pack.position.z);
                    playerObjects[i].transform.SetPositionAndRotation(new Vector3(pack.position.x, pack.position.y, pack.position.z), pack.rotation);
                    playersOnline[i].position = new Vector3(pack.position.x, pack.position.y, pack.position.z);
                    playersOnline[i].rotation = new Quaternion(pack.rotation.x, pack.rotation.y, pack.rotation.z, pack.rotation.w);

                    // playersOnline[i].playerState = pack.playerState;
                }
            }
        }

        // Wait for the next frame before processing the next packet
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
        ////Debug.Log("Received message from " + remoteEndPoint + ": " + message.message);
        Packet temp = message;
        temp.ip = remoteEndPoint;
        receivedPackets.Add(temp);

        string messageToSend = "";

        //if message.status is Connect means it's the first time
        if (message.status == Status.Connect)
        {
            messageToSend = "Welcome, " + message.user;
        }
        if (message.status == Status.Chat)
            messageToSend = message.user + " chatted: " + message.message;

        /* MESSAGE FROM SERVER TO ACKNOWLEDGE HE HAS RECEIVED SOMETHING, NOT NEEDED xd
        //messageToSend = "Moving user " + message.user + " to " + message.position;
        messageToSend = "Message received!: " + "Moving user " + message.user + " to " + message.position;
        // Example: Sending a response to the client.
        //string responseMessage = "Hello from the server!";

        Packet pack = new Packet("Server", Status.Idle, new Vector3(0, 0, 0), Quaternion.identity, messageToSend);
        pack.ip = remoteEndPoint;

        byte[] responseBytes = SerializePacket(pack);  //Encoding.UTF8.GetBytes(responseMessage);
        udpListener.Send(responseBytes, responseBytes.Length, remoteEndPoint);
        */
    }

    private void OnDestroy()
    {
        if (udpListener != null)
        {
            udpListener.Close();
        }
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