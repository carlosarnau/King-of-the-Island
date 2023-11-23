using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using UnityEngine;

public class UDPClient : MonoBehaviour
{
    public string serverIP = "127.0.0.1"; // Replace with the server's IP address.
    public int serverPort = 12345; // Specify the port the server is listening on.

    private UdpClient udpClient;

    private void Start()
    {
        try
        {
            // Create a UDP client.
            udpClient = new UdpClient();

            // Connect to the server with the provided IP and port.
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

            // Send a message to the server.
            string message = "Hello from the client!";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            udpClient.Send(messageBytes, messageBytes.Length, serverEndPoint);

            Debug.Log("UDP client connected to " + serverIP + ":" + serverPort);

            // Start listening for responses asynchronously.
            udpClient.BeginReceive(ReceiveCallback, null);
        }
        catch (SocketException e)
        {
            Debug.LogError("Error setting up UDP client: " + e.Message);
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, serverPort);
            byte[] receivedBytes = udpClient.EndReceive(ar, ref serverEndPoint);
            string responseMessage = Encoding.UTF8.GetString(receivedBytes);

            // Handle the received response from the server.
            HandleResponse(responseMessage);

            // Continue listening for more responses.
            udpClient.BeginReceive(ReceiveCallback, null);
        }
        catch (SocketException e)
        {
            Debug.LogError("UDP receive error: " + e.Message);
        }
    }

    private void HandleResponse(string responseMessage)
    {
        // Handle the received response from the server.
        Debug.Log("Received response from server: " + responseMessage);
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
}
