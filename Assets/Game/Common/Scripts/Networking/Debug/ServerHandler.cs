using System;
using UnityEngine;

namespace MultiplayerLibrary.Entity
{
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
            serverNetworkManager.Start(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]), int.Parse(args[3]));

            //int port = MatchMaker.matchMakerPort;
            //serverNetworkManager.Start(port, 0, 2, 3600);
        }

        private void Update()
        {
            serverNetworkManager.Update();
        }

        public void KickClient(int id)
        {
            serverNetworkManager.SendDisconnectClient(id);
        }
    }
}
