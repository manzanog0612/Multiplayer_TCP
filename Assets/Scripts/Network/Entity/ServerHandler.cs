using System.Net;

using UnityEngine;

public class ServerHandler : MonoBehaviour
{
    private ServerNetworkManager serverNetworkManager = null;

    private void Awake()
    {
        serverNetworkManager = new ServerNetworkManager();

        NetworkManager.Instance = serverNetworkManager;        
    }

    private void Start()
    {
        serverNetworkManager.Start();
    }

    private void Update()
    {
        serverNetworkManager.Update();
    }

    public void StartServer(IPAddress ip, int port)
    {
        if (serverNetworkManager.IsTcpConnection)
        {
            serverNetworkManager.StartTcpServer(ip, port);
        }
        else
        {
            serverNetworkManager.StartUdpServer(port);
        }
    }

    public void KickClient(int id, bool closeApp = true)
    {
        serverNetworkManager.KickClient(id, closeApp);
    }
}
