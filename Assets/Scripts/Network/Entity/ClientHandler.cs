using System.Net;

using UnityEngine;

public class ClientHandler : MonoBehaviour
{
    private ClientNetworkManager clientNetworkManager = null;
    private bool initialized = false;

    public bool Initialized { get => initialized;}
    public ClientNetworkManager ClientNetworkManager { get => clientNetworkManager; }

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
        initialized = true;

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
