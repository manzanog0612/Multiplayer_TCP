using System;
using System.Net;

public class DataHandler : MonoBehaviourSingleton<DataHandler>
{
    #region PRIVATE_FIELDS
    private bool tcpConnection = true;
    private bool isServer = false;
    #endregion

    #region ACTIONS
    public Action<PlayerData> onReceiveData;
    #endregion

    #region UNITY_CALLS
    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.onReceiveEvent -= OnReceiveDataEvent;
            NetworkManager.Instance.onStartConnection -= OnStartConnection;
        }
    }
    #endregion

    #region INITIALIZATION
    protected override void Initialize()
    {
        NetworkManager.Instance.onReceiveEvent += OnReceiveDataEvent;
        NetworkManager.Instance.onStartConnection += OnStartConnection;

        tcpConnection = NetworkManager.Instance.TcpConnection;
    }
    #endregion

    #region PUBLIC_METHODS
    public void OnReceiveDataEvent(byte[] data, IPEndPoint ip, int clientId)
    {
        if (isServer)
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

        PlayerDataMessage playerDataMessage = new PlayerDataMessage(clientId);

        onReceiveData?.Invoke(playerDataMessage.Deserialize(data));
    }

    public void SendPlayerData(PlayerData playerData)
    {
        PlayerDataMessage playerDataMessage = new PlayerDataMessage(playerData);
        byte[] message = playerDataMessage.Serialize(NetworkManager.Instance.admissionTimeStamp);
        SendData(message);
    }

    public void SendStringMessage(string chat)
    {
        StringMessage stringMessage = new StringMessage(chat);
        byte[] message = stringMessage.Serialize(NetworkManager.Instance.admissionTimeStamp);
        SendData(message);
    }

    public void SendData(byte[] message)
    {
        if (isServer)
        {
            if (tcpConnection)
            {
                NetworkManager.Instance.TcpBroadcast(message);
            }
            else
            {
                NetworkManager.Instance.UdpBroadcast(message);
            }
        }
        else
        {
            if (tcpConnection)
            {
                NetworkManager.Instance.SendToTcpServer(message);
            }
            else
            {
                NetworkManager.Instance.SendToUdpServer(message);
            }
        }
    }
    #endregion

    #region PRIVATE_METHODS
    private void OnStartConnection(bool isPlayerServer)
    {
        isServer = isPlayerServer;
    }
    #endregion
}
