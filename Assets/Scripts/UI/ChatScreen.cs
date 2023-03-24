using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ChatScreen : MonoBehaviourSingleton<ChatScreen>
{
    public Text messages;
    public InputField inputMessage;

    [NonSerialized] private bool tcpConnection = true;

    [Header("Extra")]
    public MovableSquare squareServer;
    public MovableSquare squareClient;
    public int speed;

    private void Update()
    {
        Vector2 input = Vector2.zero;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            input.x = -speed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            input.x = speed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            input.y = speed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            input.y = -speed * Time.deltaTime;
        }

        if (input != Vector2.zero)
        {
            if (NetworkManager.Instance.isServer)
            {
                squareServer.Move(input);
                Vector2Message vector2Message = new Vector2Message(squareServer.rectTransform.anchoredPosition);
                byte[] message = vector2Message.Serialize();
                SendData(message);
            }
            else
            {
                squareClient.Move(input);
                Vector2Message vector2Message = new Vector2Message(squareClient.rectTransform.anchoredPosition);
                byte[] message = vector2Message.Serialize();
                SendData(message);
            }
        }
    }

    protected override void Initialize()
    {
        inputMessage.onEndEdit.AddListener(OnEndEdit);

        this.gameObject.SetActive(false);

        NetworkManager.Instance.OnReceiveEvent += OnReceiveDataEvent;

        tcpConnection = NetworkManager.Instance.tcpConnection;
    }

    private void OnReceiveDataEvent(byte[] data, IPEndPoint ep)
    {
        if (NetworkManager.Instance.isServer)
        {
            if (tcpConnection)
            {
                NetworkManager.Instance.TcpBroadcast(data);
            }
            else
            {
                NetworkManager.Instance.UdpBroadcast(data);
            }
        }

        MessageFormater messageFormater = new MessageFormater();

        switch (messageFormater.GetMessageType(data))
        {
            case MESSAGE_TYPE.STRING:
                StringMessage stringMessage = new StringMessage();

                string chat = stringMessage.Deserialize(data);
                messages.text += chat + "\n";
                break;
            case MESSAGE_TYPE.VECTOR2:
                Vector2Message vector2Message = new Vector2Message();

                Vector2 position = vector2Message.Deserialize(data);

                if (NetworkManager.Instance.isServer)
                {
                    squareClient.SetPosition(position);
                }
                else
                {
                    squareServer.SetPosition(position);
                }
                
                break;
            default:
                break;
        }

        //string chat = Encoding.UTF8.GetString(data);
        //string[] a = chat.Split('\0');
        //
        //if (tcpConnection)
        //{
        //    messages.text += a[0]+ "\n";
        //}
        //else
        //{
        //    messages.text += Encoding.UTF8.GetString(data) + Environment.NewLine;
        //}
    }

    private void OnEndEdit(string str)
    {
        if (inputMessage.text != "")
        {           
            StringMessage stringMessage = new StringMessage(str);
            byte[] message = stringMessage.Serialize();

            SendData(message);

            if (NetworkManager.Instance.isServer)
            {
                messages.text += str + "\n";
            }

            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";
        }
    }

    private void SendData(byte[] message)
    {
        if (NetworkManager.Instance.isServer)
        {
            if (tcpConnection)
            {
                NetworkManager.Instance.TcpBroadcast(message);
                //NetworkManager.Instance.TcpBroadcast(Encoding.UTF8.GetBytes(inputMessage.text));
            }
            else
            {
                NetworkManager.Instance.UdpBroadcast(message);
                //NetworkManager.Instance.UdpBroadcast(ASCIIEncoding.UTF8.GetBytes(inputMessage.text));
            }
        }
        else
        {
            if (tcpConnection)
            {
                NetworkManager.Instance.SendToTcpServer(message);
                //NetworkManager.Instance.SendToTcpServer(Encoding.UTF8.GetBytes(inputMessage.text));
            }
            else
            {
                NetworkManager.Instance.SendToUdpServer(message);
                //NetworkManager.Instance.SendToUdpServer(ASCIIEncoding.UTF8.GetBytes(inputMessage.text));
            }
        }
    }
}
