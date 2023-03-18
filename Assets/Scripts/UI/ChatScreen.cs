using System;
using System.Net;

using UnityEngine.UI;

using ASCIIEncoding = System.Text.ASCIIEncoding;

public class ChatScreen : MonoBehaviourSingleton<ChatScreen>
{
    public Text messages;
    public InputField inputMessage;

    [NonSerialized] private bool tcpConnection = true;

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

        messages.text += ASCIIEncoding.UTF8.GetString(data) + Environment.NewLine;
    }

    private void OnEndEdit(string str)
    {
        if (inputMessage.text != "")
        {
            if (NetworkManager.Instance.isServer)
            {
                if (tcpConnection)
                {
                    NetworkManager.Instance.TcpBroadcast(ASCIIEncoding.UTF8.GetBytes(inputMessage.text));
                }
                else
                {
                    NetworkManager.Instance.UdpBroadcast(ASCIIEncoding.UTF8.GetBytes(inputMessage.text));
                }

                messages.text += inputMessage.text + Environment.NewLine;
            }
            else
            {
                if (tcpConnection)
                {
                    NetworkManager.Instance.SendToTcpServer(ASCIIEncoding.UTF8.GetBytes(inputMessage.text));
                }
                else
                {
                    NetworkManager.Instance.SendToUdpServer(ASCIIEncoding.UTF8.GetBytes(inputMessage.text));
                }
                messages.text += inputMessage.text + Environment.NewLine;
            }

            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";
        }
    }
}
