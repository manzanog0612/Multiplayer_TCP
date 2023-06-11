using System.Collections.Generic;

using UnityEngine;

using MultiplayerLibrary.Reflection.Attributes;
using MultiplayerLibrary.Entity;

[SyncNode]
public class GameManager : MonoBehaviourSingleton<GameManager>
{
    #region EXPOSED_FIELDS
    [Header("General")]
    [SerializeField] private ChatScreen chatScreen = null;

    [Header("Players")]
    [SerializeField] private PlayerHandler player = null;
    [SyncField] private Dictionary<int, string> dic = new Dictionary<int, string>();
    [SyncField] private List<char> list = new List<char>();
    [SyncField] private Queue<char> queue = new Queue<char>();
    [SyncField] private Stack<char> stack = new Stack<char>();

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
            NetworkManager.Instance.onDefineIsServer -= OnDefineIsServer;
            NetworkManager.Instance.onStartConnection -= OnStartConnection;
            NetworkManager.Instance.onAddNewClient -= OnAddNewClient;
            NetworkManager.Instance.onRemoveClient -= OnRemoveClient;
        }
    }
    #endregion

    #region INITIALIZATION
    protected override void Initialize()
    {
        NetworkManager.Instance.onDefineIsServer += OnDefineIsServer;
        NetworkManager.Instance.onStartConnection += OnStartConnection;
        NetworkManager.Instance.onAddNewClient += OnAddNewClient;
        NetworkManager.Instance.onRemoveClient += OnRemoveClient;

        chatScreen.onSendChat = OnSendChat;

        player.Init();
        player.gameObject.SetActive(false);

        dic.Add(0, "aaa");
        dic.Add(1, "bbb");
        dic.Add(2, "ccc");

        list.Add('a');
        list.Add('b');
        list.Add('c');

        queue.Enqueue('d');
        queue.Enqueue('e');
        queue.Enqueue('f');

        stack.Push('g');
        stack.Push('h');
        stack.Push('i');
    }
    #endregion

    #region PRIVATE_METHODS
    private void OnAddNewClient(int clientID, (long, float) connectionData, Vector3 position, Color color)
    {
        MovableCube cube = Instantiate(cubePrefab, cubesHolder);

        cube.SetPosition(position);
        cube.SetColor(color);

        playersSquares.Add(clientID, cube);
    }

    private void OnDefineIsServer(bool isPlayerServer)
    {
        isServer = isPlayerServer;
    }

    private void OnStartConnection()
    {
        if (!isServer)
        {
            player.gameObject.SetActive(true);
        }
    }

    //private void OnReceivePlayerData(PlayerData playerData)
    //{
    //    chatScreen.AddText(playerData.message);
    //
    //    if (!playerData.IdIsVoid() && playerData.movement != null)
    //    {
    //        playersSquares[playerData.id].SetPosition(playerData.position);
    //    }
    //}

    private void OnSendChat(string chat)
    {
        if (isServer)
        {
            chatScreen.AddText(chat);
        }
    }

    private void OnRemoveClient(int id)
    {
        if (playersSquares.ContainsKey(id))
        { 
            Destroy(playersSquares[id].gameObject);
            playersSquares.Remove(id);
        }
    }
    #endregion
}
