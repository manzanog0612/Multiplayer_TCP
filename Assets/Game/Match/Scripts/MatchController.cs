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
        //[SyncField] Dictionary<int, List<Dictionary<int, char>>> aaaaaaaaaaaaaaaaa =  new Dictionary<int, List<Dictionary<int, char>>>();
        //
        //List<Dictionary<int, char>> a = new List<Dictionary<int, char>>();
        //List<Dictionary<int, char>> b = new List<Dictionary<int, char>>();
        //
        //Dictionary<int, char> dicA = new Dictionary<int, char>();
        //Dictionary<int, char> dicB = new Dictionary<int, char>();
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

            //dicA.Add(11, 'a');
            //dicA.Add(22, 'b');
            //
            //dicB.Add(111, 'A');
            //dicB.Add(222, 'B');
            //
            //a.Add(dicA);
            //a.Add(dicB);
            //b.Add(dicB);
            //b.Add(dicA);
            //
            //aaaaaaaaaaaaaaaaa.Add(1, a);
            //aaaaaaaaaaaaaaaaa.Add(2, b);
        }

        private void Update()
        {
#if UNITY_EDITOR
#else
//if (Input.GetKeyDown(KeyCode.Space))
//{
//            if (aaaaaaaaaaaaaaaaa[1] == a)
//            {
//                aaaaaaaaaaaaaaaaa[1] = b;
//            }
//            else
//            {
//                aaaaaaaaaaaaaaaaa[1] = a;
//            }
//            }
            //Vector3 movement = Vector3.zero;
            //Vector3 rotEuler = Vector3.zero;
            //Vector3 localScale = Vector3.zero;
            //
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    if (a < aaa.Count - 1)
            //    { 
            //        a++; 
            //    }
            //    else
            //    {
            //        a = 0;
            //    }
            //}
            //
            //if (Input.GetKey(KeyCode.Y))
            //{
            //    movement.y = Time.deltaTime * 4;
            //}
            //else if (Input.GetKey(KeyCode.H))
            //{
            //    movement.y = Time.deltaTime * -4;
            //}
            //
            //if (Input.GetKey(KeyCode.G))
            //{
            //    movement.x = Time.deltaTime * -4;
            //}
            //else if (Input.GetKey(KeyCode.J))
            //{
            //    movement.x = Time.deltaTime * 4;
            //}
            //
            //if (Input.GetKey(KeyCode.O))
            //{
            //    rotEuler.z = Time.deltaTime * 40;
            //}
            //else if (Input.GetKey(KeyCode.P))
            //{
            //    rotEuler.z = Time.deltaTime * -40;
            //}
            //
            //if (Input.GetKey(KeyCode.L))
            //{
            //    rotEuler.x = Time.deltaTime * -40;
            //}
            //else if (Input.GetKey(KeyCode.K))
            //{
            //    rotEuler.x = Time.deltaTime * 40;
            //}
            //
            //if (Input.GetKey(KeyCode.R))
            //{
            //    localScale.y = Time.deltaTime * 4;
            //}
            //else if (Input.GetKey(KeyCode.T))
            //{
            //    localScale.y = Time.deltaTime * -4;
            //}
            //
            //if (Input.GetKey(KeyCode.Z))
            //{
            //    localScale.x = Time.deltaTime * -4;
            //}
            //else if (Input.GetKey(KeyCode.X))
            //{
            //    localScale.x = Time.deltaTime * 4;
            //}
            //
            //aaaaaaaaaaaaaaaaa[a].position += movement;
            //aaaaaaaaaaaaaaaaa[a].Rotate(rotEuler);
            //aaaaaaaaaaaaaaaaa[a].localScale += localScale;
#endif


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