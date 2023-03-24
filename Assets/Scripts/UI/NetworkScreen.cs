using System.Net;

using UnityEngine;
using UnityEngine.UI;

public class NetworkScreen : MonoBehaviourSingleton<NetworkScreen>
{
    #region EXPOSED_FIELDS
    [SerializeField] ChatScreen chatScreen = null;

    [SerializeField] Button connectBtn = null;
    [SerializeField] Button startServerBtn = null;
    [SerializeField] InputField portInputField = null;
    [SerializeField] InputField addressInputField = null;
    #endregion

    #region PRIVATE_FIELDS
    private bool tcpConnection = true;
    #endregion

    #region INITIALIZATION
    protected override void Initialize()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
        startServerBtn.onClick.AddListener(OnStartServerBtnClick);

        tcpConnection = NetworkManager.Instance.TcpConnection;
    }
    #endregion

    #region PRIVATE_METHODS
    private void OnConnectBtnClick()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");//addressInputField.text);
        int port = 8053;//portInputField.text);

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
        int port = 8053;//portInputField.text);)

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
        chatScreen.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
    #endregion
}
