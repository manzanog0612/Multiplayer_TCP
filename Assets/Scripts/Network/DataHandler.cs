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
            NetworkManager.Instance.OnReceiveEvent -= OnReceiveDataEvent;
            NetworkManager.Instance.onStartConnection -= OnStartConnection;
        }
    }
    #endregion

    #region INITIALIZATION
    protected override void Initialize()
    {
        NetworkManager.Instance.OnReceiveEvent += OnReceiveDataEvent;
        NetworkManager.Instance.onStartConnection += OnStartConnection;

        tcpConnection = NetworkManager.Instance.TcpConnection;
    }
    #endregion

    #region PUBLIC_METHODS
    public void OnReceiveDataEvent(byte[] data, IPEndPoint ep)
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

        PlayerDataMessage playerDataMessage = new PlayerDataMessage();

        onReceiveData.Invoke(playerDataMessage.Deserialize(data));
    }

    public void SendData(PlayerData playerData)
    {
        PlayerDataMessage playerDataMessage = new PlayerDataMessage(playerData);
        byte[] message = playerDataMessage.Serialize();
        SendData(message);
    }

    public void SendData(byte[] message)
    {
        if (isServer)
        {
            if (tcpConnection)
            {
                NetworkManager.Instance.TcpBroadcast(message);
                //NetworkManager.Instance.TcpBroadcast(Encoding.UTF8.GetBytes(inputMessage.text));
            }
            else
            {
                NetworkManager.Instance.UdpBroadcast(message);
                //NetworkManager.Instance.UdpBroadcast(ASCIIEncoding.UTF8.GetBytes(inputMessage.text));
            }
        }
        else
        {
            if (tcpConnection)
            {
                NetworkManager.Instance.SendToTcpServer(message);
                //NetworkManager.Instance.SendToTcpServer(Encoding.UTF8.GetBytes(inputMessage.text));
            }
            else
            {
                NetworkManager.Instance.SendToUdpServer(message);
                //NetworkManager.Instance.SendToUdpServer(ASCIIEncoding.UTF8.GetBytes(inputMessage.text));
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
