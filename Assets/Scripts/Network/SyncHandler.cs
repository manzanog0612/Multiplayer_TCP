using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class ConnectionTime
{
    public int id;
    public float timeSinceLastConnection = 0;
    public double latency = 0;

    public ConnectionTime(int id)
    {
        this.id = id;
    }
}

public class SyncHandler : MonoBehaviour
{
    #region EXPOSED_FIELDS
    [SerializeField] private PlayerHandler playerHandler = null;
    [SerializeField] private ServerHandler serverHandler = null;
    [SerializeField] private ClientHandler clientHandler = null;
    #endregion

    #region PRIVATE_FIELDS
    private bool tcpConnection = true;
    private bool isServer = false;

    private List<ConnectionTime> clientTimes = null; // Used by server
    private ConnectionTime serverTime = null; // Used by server

    private float timeSinceLastDataSend = 0;

    private bool canCheck = false;
    #endregion

    #region CONSTANTS
    private const int kickTime = 15;
    private const int syncTime = 2;
    #endregion

    #region UNITY_CALLS
    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.onDefineIsServer -= OnDefineIsServer;
            NetworkManager.Instance.onStartConnection -= OnStartConnection;
            NetworkManager.Instance.onAddNewClient -= OnAddNewClient;
            NetworkManager.Instance.onSync -= OnReceiveSync;
        }
    }

    private void Awake()
    {
        tcpConnection = NetworkManager.Instance.IsTcpConnection;

        NetworkManager.Instance.onDefineIsServer += OnDefineIsServer;
        NetworkManager.Instance.onStartConnection += OnStartConnection;
        NetworkManager.Instance.onAddNewClient += OnAddNewClient;
        NetworkManager.Instance.onSync += OnReceiveSync;
    }

    private void Update()
    {
        if (!canCheck)
        {
            return;
        }

        UpdateConnectionTimes();
        
        if (DataWasntSendInAWhile())
        {
            SendSyncEvent();
        }
        
        if (SomeConnectionIsLost(out int id))
        {
            if (isServer)
            {
                serverHandler.KickClient(id);
            }
            else
            {
                clientHandler.DisconectClient();
            }
        }
    }
    #endregion

    #region PRIVATE_METHODS
    private void OnDefineIsServer(bool isPlayerServer)
    {
        isServer = isPlayerServer;
    }


    private void OnStartConnection()
    {
        if (isServer)
        {
            clientTimes = new List<ConnectionTime>();
        }
        else
        {
            serverTime = new ConnectionTime(-1); // id doesn't matter because is the server
        }

        canCheck = true;
    }

    private void OnAddNewClient(int id, (long, float) connectionData, Vector3 position, Color color)
    {
        if (isServer)
        {
            clientTimes.Add(new ConnectionTime(id));
        }
    }

    private void SendSyncEvent()
    {
        SyncMessage syncMessage = new SyncMessage();

        if (isServer)
        {
            DataHandler.Instance.SendData(syncMessage.Serialize(-1));
        }
        else
        {
            DataHandler.Instance.SendData(syncMessage.Serialize((NetworkManager.Instance as ClientNetworkManager).admissionTimeStamp)); // I send this because i need to send the player id
        }

        timeSinceLastDataSend = 0;
    }

    private void OnReceiveSync(int id)
    {
        if (isServer)
        {
            ConnectionTime clientTime = clientTimes.Where(client => client.id == id).ToList()[0];
            clientTime.timeSinceLastConnection = 0;
        }
        else
        {
            serverTime.timeSinceLastConnection = 0;
        }
    }

    private bool SomeConnectionIsLost(out int id)
    {
        if (isServer)
        {
            for (int i = 0; i < clientTimes.Count; i++)
            {
                if (clientTimes[i].timeSinceLastConnection > kickTime)
                {
                    id = clientTimes[i].id;
                    clientTimes.Remove(clientTimes[i]);
                    return true;
                }
            }
        }
        else
        {
            if (serverTime.timeSinceLastConnection > kickTime)
            {
                id = serverTime.id;
                return true;
            }
        }

        id = -1;
        return false;
    }

    private bool DataWasntSendInAWhile()
    {
        return timeSinceLastDataSend > syncTime;
    }

    private void UpdateConnectionTimes()
    {
        timeSinceLastDataSend += Time.unscaledDeltaTime;

        if (isServer)
        {
            for (int i = 0; i < clientTimes.Count; i++)
            {
                clientTimes[i].timeSinceLastConnection += Time.unscaledDeltaTime;
            }
        }
        else
        {
            serverTime.timeSinceLastConnection += Time.unscaledDeltaTime;
        }
    }
    #endregion
}
