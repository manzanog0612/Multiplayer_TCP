using Game.RoomSelection.RoomsView;
using MultiplayerLibrary.Entity;
using System.Net;
using UnityEngine;

namespace Game.Common.Networking
{
    public class ClientHandler : MonoBehaviourSingleton<ClientHandler>
    {
        [SerializeField] private ClientGameNetwork clientGameNetworkManager;
        private bool initialized = false;

        public bool Initialized { get => initialized; }
        public ClientGameNetwork ClientNetworkManager { get => clientGameNetworkManager; }

        private void Awake()
        {
            clientGameNetworkManager = new ClientGameNetwork();

            NetworkManager.Instance = clientGameNetworkManager;
        }

        private void Update()
        {
            clientGameNetworkManager.Update();
        }

        public void StartClient(IPAddress ip, int port, RoomData roomData)
        {
            initialized = true;

            if (clientGameNetworkManager.IsTcpConnection)
            {
                clientGameNetworkManager.StartTcpClient(ip, port, roomData);
            }
            else
            {
                clientGameNetworkManager.StartUdpClient(ip, port, roomData);
            }
        }

        public void DisconectClient()
        {
            clientGameNetworkManager.DisconectClient();
        }
    }
}
