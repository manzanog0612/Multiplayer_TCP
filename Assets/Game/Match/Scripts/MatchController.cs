using Game.Common;
using Game.Match.Entity.Camera;
using Game.Match.Entity.Player;
using MultiplayerLibrary.Entity;
using System.Collections.Generic;
using UnityEngine;

using CharacterController = Game.Match.Entity.Player.CharacterController;

namespace Game.Match
{
    public class MatchController : SceneController
    {
        #region EXPOSED_FIELDS
        [SerializeField] private MatchView matchView = null;
        [SerializeField] private GameObject playerPrefab = null;
        [SerializeField] private Transform playersHolder = null;
        [SerializeField] private Transform[] spawnPoints = null;
        [SerializeField] private PlayerController playerController = null;
        [SerializeField] private CameraController cameraController = null;
        #endregion

        #region PRIVATE_FIELDS
        private int controlledPlayer = -1;
        private Dictionary<int, CharacterController> characterControllers = new Dictionary<int, CharacterController>();
        #endregion

        #region OVERRIDE_METHODS
        protected override void Init()
        {
            base.Init();

            matchView.Init(sessionHandler.RoomData.MatchTime);

            controlledPlayer = clientHandler.ClientId;

            SpawnPlayers();

            playerController.Init(characterControllers[controlledPlayer], clientHandler.OnGetLatency);
            cameraController.Init(characterControllers[controlledPlayer].transform);
        }
        #endregion

        #region PRIVATE_METHODS
        private void SpawnPlayers()
        {
            int i = 0;
            foreach (KeyValuePair<int, Client> client in clientHandler.Clients)
            {
                CharacterController characterController = Instantiate(playerPrefab, playersHolder).GetComponent<CharacterController>();
                characterController.Init(client.Value.color, spawnPoints[i].position);

                characterControllers.Add(client.Key, characterController);

                i++;
            }
        }
        #endregion
    }
}