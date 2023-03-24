using UnityEngine;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    #region EXPOSED_FIELDS
    [Header("General")]
    [SerializeField] private ChatScreen chatScreen = null;

    [Header("Players")]
    [SerializeField] private PlayerHandler player = null;
    #endregion

    #region PRIVATE_FIELDS
    private bool isServer = false;
    #endregion

    #region UNITY_CALLS
    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        { 
            NetworkManager.Instance.onStartConnection -= OnStartConnection; 
        }
    }
    #endregion

    #region INITIALIZATION
    protected override void Initialize()
    {
        DataHandler.Instance.onReceiveData = OnReceivePlayerData;
        NetworkManager.Instance.onStartConnection += OnStartConnection;

        chatScreen.onSendChat = OnSendChat;

        player.Init(DataHandler.Instance.SendData, chatScreen.AddText);
    }
    #endregion

    #region PRIVATE_METHODS
    private void OnStartConnection(bool isPlayerServer)
    {
        isServer = isPlayerServer;
        player.SetPlaterControlsSquare(isServer);
    }

    private void OnReceivePlayerData(PlayerData playerData)
    {
        player.SetPlayerData(playerData);
    }

    private void OnSendChat(string chat)
    {
        player.SendChat(chat);

        if (isServer)
        {
            chatScreen.AddText(chat);
        }
    }
    #endregion
}
