using MultiplayerLibrary.Entity;
using System;
using UnityEngine;

namespace Game.Common.Networking
{
    public class ServerHandler : MonoBehaviour
    {
        private ServerGameNetwork serverNetworkManager = null;

        private void Awake()
        {
            serverNetworkManager = new ServerGameNetwork();

            NetworkManager.Instance = serverNetworkManager;
        }

        private void Start()
        {
            string[] args = Environment.GetCommandLineArgs()[1].Split('-');
            serverNetworkManager.Start(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2]), int.Parse(args[3]));
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
