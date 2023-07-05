using Game.Common.Networking.Message;
using Game.Common.Requests;
using Game.RoomSelection.RoomsView;
using MultiplayerLibrary;
using MultiplayerLibrary.Entity;
using System;
using System.Collections.Generic;
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
        public Dictionary<int, Client> Clients { get => clientGameNetworkManager.Clients; }
        public int ClientId { get => clientGameNetworkManager.assignedId; }
        public Func<double> OnGetLatency { get => clientGameNetworkManager.GetLatency; }
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

        public void SendGameMessage(int clientId, GAME_MESSAGE_TYPE messageType)
        {
            clientGameNetworkManager.SendGameMessage(clientId, messageType);
        }

        public void DisconectClient()
        {
            clientGameNetworkManager.DisconectClient();
            clientGameNetworkManager.Clients.Clear();
        }

        public void SendPlayerPosition(Vector3 position)
        {
            clientGameNetworkManager.SendPlayerPosition(position);
        }

        public void SendBulletBornMessage(int id, Vector2 pos, Vector2 dir)
        {
            clientGameNetworkManager.SendBulletBornMessage(id, pos, dir);
        }

        public void SendChat(string chat)
        {
            clientGameNetworkManager.SendChat(chat);
        }

        public void SetAcions(Action<RoomData> onGetRoomData, Action onFullRoom, Action<int> onPlayersAmountChange, 
            Action<int, GAME_MESSAGE_TYPE> onReceiveGameMessage, Action<float> onTimerUpdate, Action onMatchFinished,
            Action<int, object> onReceiveMessage)
        {
            clientGameNetworkManager.SetAcions(onGetRoomData, onFullRoom, onPlayersAmountChange, onReceiveGameMessage, onTimerUpdate, onMatchFinished, onReceiveMessage);
        }
        #endregion
    }
}
