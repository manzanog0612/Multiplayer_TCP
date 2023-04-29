using System.Net;

using UnityEngine;
using UnityEngine.UI;

public class NetworkScreen : MonoBehaviourSingleton<NetworkScreen>
{
    #region EXPOSED_FIELDS
    [SerializeField] ChatScreen chatScreen = null;
    [SerializeField] ClientHandler clientHandler = null;

    [SerializeField] Button connectBtn = null;
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
        IPAddress ipAddress = IPAddress.Parse(MatchMaker.ip);
        int port = MatchMaker.matchMakerPort;

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
