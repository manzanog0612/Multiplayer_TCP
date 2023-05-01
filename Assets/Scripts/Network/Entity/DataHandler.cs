using System;
using System.Net;
using UnityEditor;

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
            NetworkManager.Instance.onDefineIsServer -= OnDefineIsServer;
        }
    }

    private void Awake()
    {
        NetworkManager.Instance.onReceiveEvent += OnReceiveDataEvent;
        NetworkManager.Instance.onDefineIsServer += OnDefineIsServer;

        tcpConnection = NetworkManager.Instance.IsTcpConnection;
    }
    #endregion

    #region PUBLIC_METHODS
    public void OnReceiveDataEvent(byte[] data, IPEndPoint ip, int clientId, MESSAGE_TYPE messageType)
    {
        PlayerDataMessage playerDataMessage = new PlayerDataMessage(clientId);

        onReceiveData?.Invoke(playerDataMessage.Deserialize(data));
    }

    public void SendPlayerData(PlayerData playerData)
    {
        PlayerDataMessage playerDataMessage = new PlayerDataMessage(playerData);
        byte[] message = playerDataMessage.Serialize((NetworkManager.Instance as ClientNetworkManager).admissionTimeStamp);
        SendData(message);
    }

    public void SendStringMessage(string chat)
    {
        StringMessage stringMessage = new StringMessage(chat);
        byte[] message = stringMessage.Serialize(-1);
        SendData(message);
    }

    public void SendData(byte[] message)
    {
        if (isServer)
        {
            if (tcpConnection)
            {
                (NetworkManager.Instance as ServerNetworkManager).TcpBroadcast(message);
            }
            else
            {
                (NetworkManager.Instance as ServerNetworkManager).UdpBroadcast(message);
            }
        }
        else
        {
            if (tcpConnection)
            {
                (NetworkManager.Instance as ClientNetworkManager).SendToTcpServer(message);
            }
            else
            {
                (NetworkManager.Instance as ClientNetworkManager).SendToUdpServer(message);
            }
        }
    }
    #endregion

    #region PRIVATE_METHODS
    private void OnDefineIsServer(bool isPlayerServer)
    {
        isServer = isPlayerServer;
    }
    #endregion
}