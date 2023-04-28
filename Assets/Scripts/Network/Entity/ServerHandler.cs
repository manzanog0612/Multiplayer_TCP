using System;
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
        NetworkManager.port = int.Parse(Environment.GetCommandLineArgs()[1]);
        serverNetworkManager.Start();
    }

    private void Update()
    {
        serverNetworkManager.Update();
    }

    public void KickClient(int id, bool closeApp = true)
    {
        serverNetworkManager.KickClient(id, closeApp);
    }
}
