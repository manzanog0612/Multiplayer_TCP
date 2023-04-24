using System.Net;

using UnityEngine;
using UnityEngine.UI;

public class NetworkScreen : MonoBehaviourSingleton<NetworkScreen>
{
    #region EXPOSED_FIELDS
    [SerializeField] ChatScreen chatScreen = null;
    [SerializeField] ClientHandler clientHandler = null;

    [SerializeField] Button connectBtn = null;
    [SerializeField] InputField portInputField = null;
    [SerializeField] InputField addressInputField = null;
    #endregion

    #region INITIALIZATION
    protected override void Initialize()
    {
        connectBtn.onClick.AddListener(OnConnectBtnClick);
    }
    #endregion

    #region PRIVATE_METHODS
    private void OnConnectBtnClick()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");// IPAddress.Parse(addressInputField.text);
        int port = 8053;// int.Parse(portInputField.text);

        clientHandler.StartClient(ipAddress, port);

        SwitchToChatScreen();
    }

    private void SwitchToChatScreen()
    {
        chatScreen.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
    #endregion
}
