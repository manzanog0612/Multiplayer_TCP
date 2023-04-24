using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using UnityEngine;

using ASCIIEncoding = System.Text.ASCIIEncoding;

public class TcpServerConnection
{
    private TcpListener listener;
    private Thread listenerThread;
    private TcpClient connectedClient;
    private IReceiveData receiver = null;
    private Queue<DataReceived> dataReceivedQueue = new Queue<DataReceived>();

    object handler = new object();

    private ConnectionData connectionData;

    public TcpServerConnection(IPAddress ip, int port, IReceiveData receiver = null)
    {
        connectionData = new ConnectionData(ip, port);

        listenerThread = new Thread(new ThreadStart(OnReceiveRequest));
        listenerThread.IsBackground = true;
        listenerThread.Start();

        this.receiver = receiver;

        Debug.Log("TCP SERVER MADE");
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

    private void OnReceiveRequest()
    {
        try
        {
            listener = new TcpListener(connectionData.server, connectionData.port);
            listener.Start();
            Debug.Log("Server is listening");
            
            while (true)
            {
                using (connectedClient = listener.AcceptTcpClient())
                {
                    // Get a stream object for reading 					
                    using (NetworkStream stream = connectedClient.GetStream())
                    {
                        int length;
                        // Read incomming stream into byte arrary. 		
                        Byte[] bytes = new Byte[1024];				
                        while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            var incommingData = new byte[length];
                            Array.Copy(bytes, 0, incommingData, 0, length);			
                            string clientMessage = ASCIIEncoding.UTF8.GetString(incommingData);
                            //Debug.Log("client message received as: " + clientMessage);
                            Debug.Log("client: " + clientMessage);

                            DataReceived dataReceived = new DataReceived((Byte[])bytes.Clone(), new IPEndPoint(connectionData.server, connectionData.port)); ;

                            for (int i = 0; i < bytes.Length; i++)
                            {
                                bytes[i] = 0;
                            }

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
            Debug.Log("SocketException " + socketException.ToString());
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
            NetworkStream stream = connectedClient.GetStream();
            if (stream.CanWrite)
            {
                stream.Write(data, 0, data.Length);
                //Debug.Log("Server sent his message - should be received by client");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }
}
