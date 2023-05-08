using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class UdpConnection
{   
    private readonly UdpClient connection;
    private IReceiveData receiver = null;
    private Queue<DataReceived> dataReceivedQueue = new Queue<DataReceived>();

    object handler = new object();
    
    public UdpConnection(int port, IReceiveData receiver = null)
    {
        connection = new UdpClient(port);
        
        this.receiver = receiver;
        
        connection.BeginReceive(OnReceive, null);
    }

    public UdpConnection(IPAddress ip, int port, IReceiveData receiver = null)
    {
        connection = new UdpClient();
        connection.Connect(ip, port);

        this.receiver = receiver;

        connection.BeginReceive(OnReceive, null);
    }

    public void Close()
    {
        connection.Close();
    }

    public void FlushReceiveData()
    {
        lock (handler)
        {
            while (dataReceivedQueue.Count > 0)
            {
                DataReceived dataReceived = dataReceivedQueue.Dequeue();
                
                receiver?.OnReceiveData(dataReceived.data, dataReceived.ipEndPoint);
            }
        }
    }

    private void OnReceive(IAsyncResult ar)
    {
        try
        {
            try
            {
                DataReceived dataReceived = new DataReceived();

                dataReceived.data = connection?.EndReceive(ar, ref dataReceived.ipEndPoint);

                lock (handler)
                {
                    dataReceivedQueue.Enqueue(dataReceived);
                }
            }
            catch (SocketException e)
            {
                // This happens when a client disconnects, as we fail to send to that port.
                UnityEngine.Debug.LogError("[UdpConnection] " + e.Message);
            }

            connection.BeginReceive(OnReceive, null);
        }
        catch (ObjectDisposedException e)
        {
            // This happens the server closes and there are clients connected, as they send a message to the server when this is closed.
            UnityEngine.Debug.Log("[UdpConnection] " + e.Message);
        }
    }

    public void Send(byte[] data)
    {
        try
        {
            connection.Send(data, data.Length);
        }
        catch (ObjectDisposedException e)
        {
            // This happens the server closes and there are clients connected, as they send a message to the server when this is closed.
            UnityEngine.Debug.Log("[UdpConnection] " + e.Message);
        }
    }

    public void Send(byte[] data, IPEndPoint ipEndpoint)
    {
        connection.Send(data, data.Length, ipEndpoint);
    }
}