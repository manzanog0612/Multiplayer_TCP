using System;
using System.Net;
using System.Text;
using UnityEngine.UI;

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

        string chat = Encoding.UTF8.GetString(data);
        string[] a = chat.Split('\0');

        if (tcpConnection)
        {
            messages.text += a[0]+ "\n";
        }
        else
        {
            messages.text += Encoding.UTF8.GetString(data) + Environment.NewLine;
        }
    }

    private void OnEndEdit(string str)
    {
        if (inputMessage.text != "")
        {
            if (NetworkManager.Instance.isServer)
            {
                if (tcpConnection)
                {
                    NetworkManager.Instance.TcpBroadcast(Encoding.UTF8.GetBytes(inputMessage.text));
                }
                else
                {
                    NetworkManager.Instance.UdpBroadcast(ASCIIEncoding.UTF8.GetBytes(inputMessage.text));
                }

                messages.text += inputMessage.text + "\n";
            }
            else
            {
                if (tcpConnection)
                {
                    NetworkManager.Instance.SendToTcpServer(Encoding.UTF8.GetBytes(inputMessage.text));
                }
                else
                {
                    NetworkManager.Instance.SendToUdpServer(ASCIIEncoding.UTF8.GetBytes(inputMessage.text));
                }
            }

            inputMessage.ActivateInputField();
            inputMessage.Select();
            inputMessage.text = "";
        }
    }
}
