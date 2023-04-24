using System.Net;

using UnityEngine;

public class ClientHandler : MonoBehaviour
{
    private ClientNetworkManager clientNetworkManager = null;

    private void Awake()
    {
        clientNetworkManager = new ClientNetworkManager();

        NetworkManager.Instance = clientNetworkManager;
    }

    private void Update()
    {
        clientNetworkManager.Update();
    }

    public void StartClient(IPAddress ip, int port)
    {
        if (clientNetworkManager.IsTcpConnection)
        {
            clientNetworkManager.StartTcpClient(ip, port);
        }
        else
        {
            clientNetworkManager.StartUdpClient(ip, port);
        }
    }

    public void DisconectClient()
    {
        clientNetworkManager.DisconectClient();
    }
}
