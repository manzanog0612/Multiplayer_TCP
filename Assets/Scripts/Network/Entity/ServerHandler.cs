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
        string[] args = Environment.GetCommandLineArgs()[1].Split('-');

        serverNetworkManager.Start(int.Parse(args[0]), int.Parse(args[1]));
        
        //int port = MatchMaker.matchMakerPort;
        //serverNetworkManager.Start(port, 0);
    }

    private void Update()
    {
        serverNetworkManager.Update();

        //if (Input.GetKeyDown(KeyCode.W))
        //{
        //    serverNetworkManager.ShutDownUdpServer();
        //}
    }

    public void KickClient(int id)
    {
        serverNetworkManager.SendDisconnect(id);
    }
}
