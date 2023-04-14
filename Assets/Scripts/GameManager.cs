using System.Collections.Generic;

using UnityEngine;

public class GameManager : MonoBehaviourSingleton<GameManager>
{
    #region EXPOSED_FIELDS
    [Header("General")]
    [SerializeField] private ChatScreen chatScreen = null;

    [Header("Players")]
    [SerializeField] private PlayerHandler player = null;

    [Header("Game Configurations")]
    [SerializeField] private MovableCube cubePrefab = null;
    [SerializeField] private Transform cubesHolder = null;
    #endregion

    #region PRIVATE_FIELDS
    private bool isServer = false;

    private Dictionary<int, MovableCube> playersSquares = new Dictionary<int, MovableCube>();
    #endregion

    #region UNITY_CALLS
    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        { 
            NetworkManager.Instance.onStartConnection -= OnStartConnection;
            NetworkManager.Instance.onAddNewClient -= OnAddNewClient;
        }

        if (DataHandler.Instance != null)
        {
            DataHandler.Instance.onReceiveData -= OnReceivePlayerData;
        }
    }
    #endregion

    #region INITIALIZATION
    protected override void Initialize()
    {
        DataHandler.Instance.onReceiveData += OnReceivePlayerData;
        NetworkManager.Instance.onStartConnection += OnStartConnection;
        NetworkManager.Instance.onAddNewClient += OnAddNewClient;

        chatScreen.onSendChat = OnSendChat;

        player.Init(DataHandler.Instance.SendPlayerData);
        player.gameObject.SetActive(false);
    }
    #endregion

    #region PRIVATE_METHODS
    private void OnAddNewClient(int clientID, Vector3 position, Color color)
    {
        MovableCube cube = Instantiate(cubePrefab, cubesHolder);

        cube.SetPosition(position);
        cube.SetColor(color);

        playersSquares.Add(clientID, cube);
    }

    private void OnStartConnection(bool isPlayerServer)
    {
        isServer = isPlayerServer;

        if (!isServer)
        {
            player.gameObject.SetActive(true);
        }
    }

    private void OnReceivePlayerData(PlayerData playerData)
    {
        chatScreen.AddText(playerData.message);

        if (!playerData.IdIsVoid() && playerData.movement != null)
        {
            playersSquares[playerData.id].SetPosition(playerData.position);
        }
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
