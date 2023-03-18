using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class TcpServerConnection : MonoBehaviour
{
    private TcpListener listener;
    private Thread listenerThread;
    private TcpClient connectedClient;
    private IReceiveData receiver = null;
    private Queue<DataReceived> dataReceivedQueue = new Queue<DataReceived>();

    object handler = new object();

    private ConnectionData connectionData;

    private Logger logger;

    public TcpServerConnection(string server, int port, IReceiveData receiver = null, Logger logger = null)
    {
        connectionData = new ConnectionData(server, port);

        listenerThread = new Thread(new ThreadStart(OnReceiveRequest));
        listenerThread.IsBackground = true;
        listenerThread.Start();

        this.receiver = receiver;

        this.logger = logger;
        logger.SendLog("TCP SERVER MADE");
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

    private void OnReceiveRequest()
    {
        try
        {
            listener = new TcpListener(IPAddress.Parse(connectionData.server), connectionData.port);
            listener.Start();
            logger.SendLog("Server is listening");
            Byte[] bytes = new Byte[1024];
            while (true)
            {
                using (connectedClient = listener.AcceptTcpClient())
                {
                    // Get a stream object for reading 					
                    using (NetworkStream stream = connectedClient.GetStream())
                    {
                        int length;
                        // Read incomming stream into byte arrary. 						
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);
                            // Convert byte array to string message. 							
                            string clientMessage = Encoding.ASCII.GetString(incommingData);
                            Debug.Log("client message received as: " + clientMessage);

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
        }
        catch (SocketException socketException)
        {
            logger.SendLog("SocketException " + socketException.ToString());
        }
    }

    public void Send(byte[] data)
    {
        if (connectedClient == null)
        {
            return;
        }
        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = connectedClient.GetStream();
            if (stream.CanWrite)
            {
                string serverMessage = "This is a message from your server.";
                // Convert string message to byte array.                 
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage);
                // Write byte array to socketConnection stream.               
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                Debug.Log("Server sent his message - should be received by client");

                //stream.Write(data, 0, data.Length);
                logger.SendLog("Server sent his message");
            }
        }
        catch (SocketException socketException)
        {
            logger.SendLog("Socket exception: " + socketException);
        }
    }
}
