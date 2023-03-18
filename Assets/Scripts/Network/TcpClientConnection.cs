using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpClientConnection
{
    private TcpClient connection;
    private Thread receiveThread;
    private IReceiveData receiver = null;
    private Queue<DataReceived> dataReceivedQueue = new Queue<DataReceived>();

    object handler = new object();

    private ConnectionData connectionData;

    private Logger logger;

    public TcpClientConnection(string server, int port, IReceiveData receiver = null, Logger logger = null)
    {
        try
        {
            connectionData = new ConnectionData("localhost", port);

            receiveThread = new Thread(new ThreadStart(OnReceive));
            receiveThread.IsBackground = true;
            receiveThread.Start();

            this.receiver = receiver;
            this.logger = logger;
            logger.SendLog("TCP CLIENT MADE");
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }

    public void FlushReceiveData()
    {
        lock (handler)
        {
            while (dataReceivedQueue.Count > 0)
            {
                DataReceived dataReceived = dataReceivedQueue.Dequeue();
                if (receiver != null)
                    receiver.OnReceiveDataTcp(dataReceived.data, dataReceived.ipEndPoint);
            }
        }
    }

    private void OnReceive()
    {
        try
        {
            connection = new TcpClient(new IPEndPoint(IPAddress.Parse(connectionData.server), connectionData.port));
            Byte[] bytes = new Byte[1024];
            while (true)
            {		
                using (NetworkStream stream = connection.GetStream())
                {
                    int length;
                    // Read incomming stream into byte arrary. 					
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        // Convert byte array to string message. 						
                        string serverMessage = Encoding.ASCII.GetString(incommingData);
                        Debug.Log("server message received as: " + serverMessage);

                        DataReceived dataReceived = new DataReceived();
                        dataReceived.data = bytes;

                        lock (handler)
                        {
                            dataReceivedQueue.Enqueue(dataReceived);
                        }
                    }
                }
            }
        }
        catch (SocketException socketException)
        {
            logger.SendLog("Socket exception: " + socketException);
        }
    }

    public void Send(byte[] data)
    {
        if (connection == null)
        {
            return;
        }
        try
        {	
            NetworkStream stream = connection.GetStream();
            if (stream.CanWrite)
            {
                string clientMessage = "This is a message from one of your clients.";
                // Convert string message to byte array.                 
                byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(clientMessage);
                // Write byte array to socketConnection stream.                 
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
                Debug.Log("Client sent his message - should be received by server");

                //stream.Write(data, 0, data.Length);

                logger.SendLog("Client sent his message");
            }
        }
        catch (SocketException socketException)
        {
            logger.SendLog("Socket exception: " + socketException);
        }
    }
}
