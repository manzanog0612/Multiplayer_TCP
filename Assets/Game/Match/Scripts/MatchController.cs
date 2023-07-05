using Game.Common;
using Game.Common.Requests;
using Game.Match.Controllers;
using Game.Match.Entity.Camera;
using Game.Match.Entity.Player;
using MultiplayerLibrary.Interfaces;
using MultiplayerLibrary.Reflection;
using MultiplayerLibrary.Reflection.Attributes;
using System;
using System.Collections.Generic;
using UnityEngine;

using CharacterController = Game.Match.Entity.Player.CharacterController;
using Client = MultiplayerLibrary.Entity.Client;

namespace Game.Match
{
    public class MatchController : SceneController, ISync
    {
        #region EXPOSED_FIELDS
        [SerializeField] private MatchView matchView = null;
        [SerializeField] private GameObject playerPrefab = null;
        [SerializeField] private Transform playersHolder = null;
        [SerializeField] private Transform[] spawnPoints = null;
        [SerializeField] private PlayerController playerController = null;
        [SerializeField] private CameraController cameraController = null;
        [SerializeField] private ReflectionHandler reflectionHandler = null;

        [Header("Controllers")]
        [SerializeField] private TurretsController turretsController = null;
        #endregion

        #region PRIVATE_FIELDS
        private int controlledPlayer = -1;
        private Dictionary<int, CharacterController> characterControllers = new Dictionary<int, CharacterController>();
        private bool matchEnded = false;
        #endregion

        #region OVERRIDE_METHODS
        protected override void Init()
        {
            base.Init();

            matchView.Init(sessionHandler.RoomData.MatchTime, OnGoBack, clientHandler.SendChat);

            controlledPlayer = clientHandler.ClientId;

            SpawnPlayers();
            turretsController.Init(clientHandler.SendBulletBornMessage,
                onSetBulletDeath: (bulletId, collision) =>
                {
                    OnPlayerHitEffective(collision);
                    clientHandler.SendGameMessage(bulletId, GAME_MESSAGE_TYPE.BULLET_DEATH);
                });

            playerController.Init(characterControllers[controlledPlayer], clientHandler.OnGetLatency,
                onSendMessage: (messageType) =>
                {
                    clientHandler.SendGameMessage(controlledPlayer, messageType);
                });

            cameraController.Init(characterControllers[controlledPlayer].transform);

            reflectionHandler.SetEntryPoint(this);
            sessionHandler.SetOnReceiveGameMessage(OnReceiveGameMessage);
            sessionHandler.SetOnPlayersAmountChange(OnClientDisconnected);
            sessionHandler.SetOnUpdateTimer(matchView.UpdateTimer);
            sessionHandler.onReceiveMessage = OnReceiveMessage;
            sessionHandler.onMatchFinished = FinishMatch;
        }

        private void Update()
        {
            if (matchEnded)
            {
                return;
            }

            clientHandler.SendPlayerPosition(characterControllers[controlledPlayer].transform.position);
        }

        public byte[] Serialize()
        {
            List<byte> bytes = new List<byte>();

            foreach (KeyValuePair<int, CharacterController> character in characterControllers)
            {
                bytes.AddRange(BitConverter.GetBytes(character.Key));
                bytes.AddRange(character.Value.Serialize());
            }

            return bytes.ToArray();
        }

        public void Deserialize(byte[] msg)
        {
            if (msg.Length == 0)
            {
                return;
            }

            int offset = 0;

            for (int i = 0; i < characterControllers.Count; i++)
            {
                int key = BitConverter.ToInt32(msg, offset);
                offset += sizeof(int);

                List<byte> bytes = new List<byte>();

                for (int j = 0; j < sizeof(float) * 6; j++)
                {
                    bytes.Add(msg[offset + j]);
                }

                offset += sizeof(float) * 6;

                characterControllers[key].Deserialize(bytes.ToArray());
            }
        }
#endregion

#region PRIVATE_METHODS
        private void OnReceiveMessage(int messageType, object data)
        {
            if (messageType == (int)MESSAGE_TYPE.STRING)
            {
                matchView.AddTextToChatScreen((string)data);
            }
            else
            {
                turretsController.OnReceiveTurretData(messageType, data);
            }
        }

        private void SpawnPlayers()
        {
            int i = 0;
            foreach (KeyValuePair<int, Client> client in clientHandler.Clients)
            {
                CharacterController characterController = Instantiate(playerPrefab, playersHolder).GetComponent<CharacterController>();

                characterController.gameObject.name = characterController.gameObject.name + client.Key;
                characterController.Init(client.Value.color, spawnPoints[i].position, clientHandler.ClientId != client.Key, OnPlayerHitEffective);

                characterControllers.Add(client.Key, characterController);

                i++;
            }
        }

        private void OnPlayerHitEffective(Collider2D collider2D)
        {
            CharacterController hitCharacter = collider2D.gameObject.GetComponent<CharacterController>();
            int id = -1;

            foreach (KeyValuePair<int, CharacterController> character in characterControllers)
            {
                if (characterControllers[character.Key] == hitCharacter)
                {
                    id = character.Key;
                    break;
                }
            }

            if (id != -1)
            {
                clientHandler.SendGameMessage(id, GAME_MESSAGE_TYPE.PLAYER_HIT_EFFECTIVE);
            }
            else
            {
                Debug.Log("Player wasn't hit");
            }
        }

        private void OnReceiveGameMessage(int clientId, GAME_MESSAGE_TYPE messageType)
        {
            switch (messageType)
            {
                case GAME_MESSAGE_TYPE.PLAYER_HIT:
                    characterControllers[clientId].DetectHitAction(true, true);
                    break;
                case GAME_MESSAGE_TYPE.PLAYER_HIT_EFFECTIVE:
                    characterControllers[clientId].TakeHit();

                    if (clientId == controlledPlayer)
                    {
                        CheckPlayerAlive();
                    }
                    break;
                default:
                    break;
            }
        }

        private void FinishMatch()
        {
            int higherLife = -1;
            int idHigherLife = 0;

            foreach (KeyValuePair<int, CharacterController> character in characterControllers)
            {
                if (character.Value.Life > higherLife)
                {
                    higherLife = character.Value.Life;
                    idHigherLife = character.Key;
                }
            }

            playerController.gameObject.SetActive(false);
            matchEnded = true;
            matchView.SetResultView(idHigherLife == clientHandler.ClientId);
            clientHandler.DisconectClient();
        }

        private void OnClientDisconnected()
        {
            foreach (KeyValuePair<int, CharacterController> character in characterControllers)
            {
                int characterKey = character.Key;

                if (!clientHandler.Clients.ContainsKey(characterKey))
                {
                    CharacterController characterController = characterControllers[characterKey];
                    characterControllers.Remove(characterKey);
                    Destroy(characterController.gameObject);
                    break;
                }
            }

            if (clientHandler.Clients.Count == 1) //only this client alive
            {
                FinishMatch();
            }
        }

        private void OnGoBack()
        {
            ChangeScene(SCENES.LOGIN);
        }

        private void CheckPlayerAlive()
        {
            if (characterControllers[controlledPlayer].Life < 0)
            {
                FinishMatch();
            }
        }
#endregion
    }
}