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
    [SerializeField] private MovableSquare squarePrefab = null;
    [SerializeField] private Transform squaresHolder = null;
    #endregion

    #region PRIVATE_FIELDS
    private bool isServer = false;

    private Dictionary<int,MovableSquare> playersSquares = new Dictionary<int, MovableSquare>();
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
    }
    #endregion

    #region PRIVATE_METHODS
    private void OnAddNewClient(int clientID)
    {
        MovableSquare square = Instantiate(squarePrefab, squaresHolder);

        square.gameObject.transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        square.SetText(clientID.ToString());

        playersSquares.Add(clientID, square);
    }

    private void OnStartConnection(bool isPlayerServer)
    {
        isServer = isPlayerServer;

        if (isServer)
        {
            player.gameObject.SetActive(false);
        }
        else
        {
            player.TurnOnSquare();
        }
    }

    private void OnReceivePlayerData(PlayerData playerData)
    {
        chatScreen.AddText(playerData.message);

        if (!playerData.IdIsVoid() && playerData.movement != null)
        {
            playersSquares[playerData.id].Move((Vector2)playerData.movement);
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
