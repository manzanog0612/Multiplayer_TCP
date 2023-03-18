using System;
using System.Net;

using UnityEngine.UI;

public class NetworkScreen : MonoBehaviourSingleton<NetworkScreen>
{
    public Button connectBtn;
    public Button startServerBtn;
    public InputField portInputField;
    public InputField addressInputField;

    [NonSerialized] private bool tcpConnection = true;

    protected override void Initialize()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
        startServerBtn.onClick.AddListener(OnStartServerBtnClick);

        tcpConnection = NetworkManager.Instance.tcpConnection;
    }

    private void OnConnectBtnClick()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");//addressInputField.text);
        int port = System.Convert.ToInt32("25565");//portInputField.text);

        if (tcpConnection)
        {
            NetworkManager.Instance.StartTcpClient(ipAddress, port);
        }
        else
        {
            NetworkManager.Instance.StartUdpClient(ipAddress, port);
        }
       
        
        SwitchToChatScreen();
    }

    private void OnStartServerBtnClick()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");//addressInputField.text);
        int port = System.Convert.ToInt32("25565");//portInputField.text);

        if (tcpConnection)
        {
            NetworkManager.Instance.StartTcpServer(ipAddress, port);//addressInputField.text, port);
        }
        else
        {
            NetworkManager.Instance.StartUdpServer(port);
        }

        SwitchToChatScreen();
    }

    private void SwitchToChatScreen()
    {
        ChatScreen.Instance.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
