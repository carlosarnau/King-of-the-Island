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

    private UdpClient udpListener;

    private void Start()
    {
        try
        {
            // Create a UDP listener on the specified port.
            udpListener = new UdpClient(port);
            Debug.Log("UDP server is listening on port " + port);

            // Start listening for incoming messages asynchronously.
            udpListener.BeginReceive(ReceiveCallback, null);
        }
        catch (SocketException e)
        {
            Debug.LogError("Error setting up UDP server: " + e.Message);
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            byte[] receivedBytes = udpListener.EndReceive(ar, ref remoteEndPoint);
            string message = Encoding.UTF8.GetString(receivedBytes);

            // Handle the received message, e.g., respond to the client.
            HandleMessage(message, remoteEndPoint);

            // Continue listening for more messages.
            udpListener.BeginReceive(ReceiveCallback, null);
        }
        catch (SocketException e)
        {
            Debug.LogError("UDP receive error: " + e.Message);
        }
    }

    private void HandleMessage(string message, IPEndPoint remoteEndPoint)
    {
        // Handle the received message here. You can send a response back to the client if needed.
        Debug.Log("Received message from " + remoteEndPoint + ": " + message);

        // Example: Sending a response to the client.
        string responseMessage = "Hello from the server!";
        byte[] responseBytes = Encoding.UTF8.GetBytes(responseMessage);
        udpListener.Send(responseBytes, responseBytes.Length, remoteEndPoint);
    }

    private void OnDestroy()
    {
        if (udpListener != null)
        {
            udpListener.Close();
        }
    }
}
