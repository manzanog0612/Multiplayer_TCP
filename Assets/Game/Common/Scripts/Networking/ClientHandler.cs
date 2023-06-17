using Game.RoomSelection.RoomsView;
using MultiplayerLibrary;
using MultiplayerLibrary.Entity;
using System;
using System.Net;
using UnityEngine;

namespace Game.Common.Networking
{
    public class ClientHandler : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private ClientGameNetwork clientGameNetworkManager;
        #endregion

        #region PRIIVATE_FIELDS
        private bool initialized = false;
        #endregion

        #region PROPERTIES
        public bool Initialized { get => initialized; }
        public ClientGameNetwork ClientNetworkManager { get => clientGameNetworkManager; }
        #endregion

        #region UNITY_CALLS
        private void Awake()
        {
            clientGameNetworkManager = new ClientGameNetwork();

            NetworkManager.Instance = clientGameNetworkManager;
        }

        private void Update()
        {
            clientGameNetworkManager.Update();
        }
        #endregion

        #region PUBLIC_METHODS
        public void StartClient()
        {
            IPAddress ipAddress = IPAddress.Parse(MatchMaker.ip);
            int port = MatchMaker.matchMakerPort;

            initialized = true;

            if (clientGameNetworkManager.IsTcpConnection)
            {
                clientGameNetworkManager.StartTcpClient(ipAddress, port);
            }
            else
            {
                clientGameNetworkManager.StartUdpClient(ipAddress, port);
            }
        }

        public void StartMatchMakerConnection(RoomData roomData)
        {
            initialized = true;
            //DEBUG comentar
            clientGameNetworkManager.SendConnectRequest(roomData);
            //DEBUG descomentar
            //clientGameNetworkManager.SendHandShake(); 
        }

        public void SendRoomsDataRequest(Action<RoomData[]> onReceiveRoomDatas)
        {
            initialized = true;
            //DEBUG comentar
            clientGameNetworkManager.SendRoomDatasRequest(onReceiveRoomDatas);
        }

        public void DisconectClient()
        {
            clientGameNetworkManager.DisconectClient();
        }

        public void SetAcions(Action<RoomData> onGetRoomData, Action onFullRoom, Action<int> onPlayersAmountChange)
        {
            clientGameNetworkManager.SetAcions(onGetRoomData, onFullRoom, onPlayersAmountChange);
        }
        #endregion
    }
}
