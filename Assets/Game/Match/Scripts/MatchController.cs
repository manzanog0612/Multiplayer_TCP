using Game.Common;
using Game.Common.Requests;
using Game.Match.Controllers;
using Game.Match.Entity.Camera;
using Game.Match.Entity.Player;
using MultiplayerLibrary.Reflection;
using MultiplayerLibrary.Reflection.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

        //[SerializeField] private CharacterController characterController = null;
        #endregion

        #region PRIVATE_FIELDS
        private int controlledPlayer = -1;
        private Dictionary<int, CharacterController> characterControllers = new Dictionary<int, CharacterController>();
        //[SyncField]private Dictionary<int, Dictionary<string, float>> dic = new Dictionary<int, Dictionary<string, float>>();
        #endregion

        #region OVERRIDE_METHODS
        protected override void Init()
        {
            base.Init();

            matchView.Init(sessionHandler.RoomData.MatchTime, OnGoBack);

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
                });//null, null);

            cameraController.Init(characterControllers[controlledPlayer].transform);

            //characterController.Init(Color.red, spawnPoints[0].position, false);//

            //dic.Add(1, new Dictionary<string, float> { ["a"] = 0.1f, ["aa"] = 0.11f });
            //dic.Add(2, new Dictionary<string, float> { ["b"] = 0.2f, ["bb"] = 0.22f });

            reflectionHandler.SetEntryPoint(this);

            sessionHandler.SetOnReceiveGameMessage(OnReceiveGameMessage);
            sessionHandler.SetOnPlayersAmountChange(OnClientDisconnected);
            sessionHandler.SetOnUpdateTimer(matchView.UpdateTimer);
            sessionHandler.onReceiveServerGameMessage = turretsController.OnReceiveTurretData;
            sessionHandler.onMatchFinished = OnFinishMatch;
        }

        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    if (dic[1] == new Dictionary<string, float> { ["a"] = 0.1f, ["aa"] = 0.11f })
            //    {
            //        dic[1] = new Dictionary<string, float> { ["b"] = 0.2f, ["bb"] = 0.22f };
            //    }
            //    else
            //    {
            //        dic[1] = new Dictionary<string, float> { ["a"] = 0.1f, ["aa"] = 0.11f };
            //    }
            //}

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
                    break;
                default:
                    break;
            }
        }

        private void OnFinishMatch()
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

            matchView.SetResultView(idHigherLife == clientHandler.ClientId);
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
        }

        private void OnGoBack()
        {
            clientHandler.DisconectClient();

            ChangeScene(SCENES.LOGIN);
        }

        private Transform OnGetPlayerTransform(int id)
        {
            if (characterControllers.ContainsKey(id))
            {
                return characterControllers[id].GetComponent<Transform>();
            }

            return null;
        }
        #endregion
    }
}