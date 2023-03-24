using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

using ASCIIEncoding = System.Text.ASCIIEncoding;
public class TcpClientConnection
{
    private TcpClient connection;
    private Thread receiveThread;
    private IReceiveData receiver = null;
    private Queue<DataReceived> dataReceivedQueue = new Queue<DataReceived>();

    object handler = new object();

    private ConnectionData connectionData;

    private Logger logger;

    public TcpClientConnection(IPAddress ip, int port, IReceiveData receiver = null, Logger logger = null)
    {
        try
        {
            connectionData = new ConnectionData(ip, port);

            receiveThread = new Thread(new ThreadStart(OnReceive));
            receiveThread.IsBackground = false;
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
            connection = new TcpClient("localhost", connectionData.port);//new TcpClient("localhost", 8052);//;
            while (true)
            {		
                using (NetworkStream stream = connection.GetStream())
                {
                    int length;
                    Byte[] bytes = new Byte[1024];
                    // Read incomming stream into byte arrary. 					
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);		
                        string serverMessage = ASCIIEncoding.UTF8.GetString(incommingData);
                        //logger.SendLog("server message received as: " + serverMessage);
                        logger.SendLog("server: " + serverMessage);

                        DataReceived dataReceived = new DataReceived((Byte[])bytes.Clone(), new IPEndPoint(connectionData.server, connectionData.port));

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
                stream.Write(data, 0, data.Length);
                //logger.SendLog("Client sent his message - should be received by server");
            }
        }
        catch (SocketException socketException)
        {
            logger.SendLog("Socket exception: " + socketException);
        }
    }
}
