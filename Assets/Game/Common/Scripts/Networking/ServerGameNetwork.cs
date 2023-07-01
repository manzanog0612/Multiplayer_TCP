using Game.Common.Networking.Message;
using Game.Common.Requests;
using MultiplayerLib2.Network.Message;
using MultiplayerLibrary.Entity;
using MultiplayerLibrary.Interfaces;
using MultiplayerLibrary.Message;
using MultiplayerLibrary.Message.Formater;
using MultiplayerLibrary.Network.Message.Constants;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace Game.Common.Networking
{
    public class ServerGameNetwork : ServerNetworkManager
    {
        #region PRIVATE_FIELDS
        private bool matchStarted = false;
        private bool matchEnded = false;
        private float matchTime = 0;

        private Dictionary<int, Vector2> playersPositions = new Dictionary<int, Vector2>();
        private List<ServerBullet> bullets = new List<ServerBullet>();
        private List<ServerTurret> turrets = new List<ServerTurret>();
        #endregion

        #region OVERRIDE_METHODS
        public override void Start(int port, int id, int playersMax, int matchTime)
        {
            base.Start(port, id, playersMax, matchTime);

            this.matchTime = matchTime;            
        }

        public override void Update()
        {
            base.Update();

            UpdateMatch();
        }

        public override void OnReceiveData(byte[] data, IPEndPoint ip)
        {
            base.OnReceiveData(data, ip);

            bool isReflectionMessage = MessageFormater.IsReflectionMessage(data);

            if (isReflectionMessage)
            {
                return;
            }
            else
            {
                GAME_MESSAGE_TYPE messageType = GameMessageFormater.GetMessageType(data);

                switch (messageType)
                {
                    case GAME_MESSAGE_TYPE.PLAYER_POS:
                        ProcessPlayerPosition(data, ip);
                        break;
                    case GAME_MESSAGE_TYPE.BULLET_BORN:
                        ProcessBulletBornMessage(data, ip);
                        break;
                    default:
                        break;
                }
            }
        }
        #endregion

        #region DATA_RECEIVE_PROCESS
        private void ProcessBulletBornMessage(byte[] data, IPEndPoint ip)
        {
            BulletBornMessage bulletBornMessage = new BulletBornMessage(BulletBornMessage.Deserialize(data));
            HandleMessageError(data, (int)GAME_MESSAGE_TYPE.BULLET_BORN, bulletBornMessage, BulletBornMessage.GetMessageSize(), BulletBornMessage.GetHeaderSize());
            
            if (!wasLastMessageSane)
            {
                SendResendDataMessage((int)GAME_MESSAGE_TYPE.BULLET_BORN, ip);
                return;
            }

            (int id, Vector2 pos, Vector2 dir) = BulletBornMessage.Deserialize(data);

            if (bullets.Find(b => b.id == id) == null)
            { 
                bullets.Add(new ServerBullet(pos, dir, TurretsConstants.bulletSpeed, id)); 
            }
        }

        private void ProcessPlayerPosition(byte[] data, IPEndPoint ip)
        {
            CheckIfMessageIdIsCorrect(data, (int)GAME_MESSAGE_TYPE.PLAYER_POS);

            if (!wasLastMessageSane)
            {
                return;
            }

            (int playerId, Vector2 position) = PlayerPositionMessage.Deserialize(data);

            if (!playersPositions.ContainsKey(playerId))
            {
                playersPositions.Add(playerId, position);
            }
            else
            {
                playersPositions[playerId] = position;
            }

            CheckAnyPlayerToShoot();
        }

        protected override void ProcessEntityDisconnect(IPEndPoint ip, byte[] data)
        {
            base.ProcessEntityDisconnect(ip, data);

            if (!wasLastMessageSane)
            {
                return;
            }

            int clientId = RemoveEntityMessage.Deserialize(data);

            if (playersPositions.ContainsKey(clientId))
            {
                playersPositions.Remove(clientId);
            }

            if (clients.Count == 0 && (matchEnded || matchStarted))
            {
                Application.Quit();
            }
        }

        protected override void ProcessGameMessage(IPEndPoint ip, byte[] data)
        {
            base.ProcessGameMessage(ip, data);

            if (!wasLastMessageSane)
            {
                return;
            }

            (int id, int message) = GameMessage.Deserialize(data);

            switch ((GAME_MESSAGE_TYPE)message)
            {
                case GAME_MESSAGE_TYPE.BULLET_DEATH:
                    ProcessBulletDeath(id);
                    break;
                default:
                    break;
            }
        }

        protected override void ProcessHandShake((IPEndPoint ip, float timeStamp) clientConnectionData, byte[] data)
        {
            base.ProcessHandShake(clientConnectionData, data);

            if (!wasLastMessageSane)
            {
                return;
            }

            if (clients.Count == roomData.PlayersMax)
            {
                InitMatch();
            }
        }
        #endregion

        #region SEND_DATA_METHODS
        private void SendBulletPosition(int bulletId, Vector2 position)
        {
            BulletPositionMessage bulletPositionMessage = new BulletPositionMessage((bulletId, position));
            byte[] data = bulletPositionMessage.Serialize();

            SendData(data);
        }

        private void SendTurretShotMessage(int turretId, int bulletId)
        {
            TurretShootMessage turretShotMessage = new TurretShootMessage((turretId, bulletId));
            byte[] data = turretShotMessage.Serialize();

            SaveSentMessage((int)GAME_MESSAGE_TYPE.TURRET_SHOOT, data, GetBiggerLatency() * latencyMultiplier);
            SendData(data);
        }

        private void SendTurretRotation(int turretId, Quaternion rotation)
        {
            TurretRotationMessage turretRotationMessage = new TurretRotationMessage((turretId, turrets[turretId].rot));
            byte[] data = turretRotationMessage.Serialize();

            SendData(data);
        }

        private void SendTimerUpdateMessage()
        {
            TimerMessage timerMessage = new TimerMessage(matchTime < 0 ? 0 : matchTime);

            byte[] data = timerMessage.Serialize();
            SendData(data);
        }

        private void SendNotice(NOTICE notice)
        {
            NoticeMessage noticeMessage = new NoticeMessage((int)notice);
            byte[] data = noticeMessage.Serialize();

            SaveSentMessage((int)MESSAGE_TYPE.NOTICE, data, GetBiggerLatency() * latencyMultiplier);
            SendData(data);
        }
        #endregion

        #region PRIVATE_METHODS
        #region MATCH_PROCESS
        private void InitMatch()
        {
            roomData.InMatch = true;
            SendNotice(NOTICE.FULL_ROOM);
            matchStarted = true;
            InitTurrets();
        }

        private void InitTurrets()
        {
            Vector2[] turretsPos = TurretsConstants.GetTurretsPos();

            turrets.Clear();

            for (int i = 0; i < turretsPos.Length; i++)
            {
                ServerTurret turret = new ServerTurret(i, turretsPos[i], Quaternion.identity, TurretsConstants.cooldown, SendTurretShotMessage, SendTurretRotation);
                turrets.Add(turret);
            }
        }

        private void UpdateMatch()
        {
            if (!matchStarted || matchEnded)
            {
                return;
            }

            matchTime -= Time.deltaTime;
            SendTimerUpdateMessage();

            if (matchTime < 0)
            {
                matchEnded = true;
                SendNotice(NOTICE.MATCH_FINISHED);
            }
            else
            {
                for (int i = 0; i < turrets.Count; i++)
                {
                    turrets[i].Update();
                }

                for (int i = 0; i < bullets.Count; i++)
                {
                    bullets[i].Update();
                    SendBulletPosition(bullets[i].id, bullets[i].pos);
                }
            }
        }
        #endregion

        private void CheckAnyPlayerToShoot()
        {
            for (int i = 0; i < turrets.Count; i++)
            {
                int playerId = GetCloserPlayer(turrets[i].pos);

                if (playerId != -1 && !turrets[i].rotating)
                {
                    Quaternion lookRot = turrets[i].GetLookRot(playersPositions[playerId]);
                    turrets[i].StartRotation(lookRot, TurretsConstants.rotDuration);
                }
            }
        }

        private int GetCloserPlayer(Vector2 pos)
        {
            int closerPLayerId = -1;
            float minDistance = TurretsConstants.minDistanceToShoot;

            foreach(var player in playersPositions)
            {
                float distance = Vector2.Distance(player.Value, pos);

                if (distance < minDistance)
                {
                    closerPLayerId = player.Key;
                    minDistance = distance;
                }
            }

            return closerPLayerId;
        }

        private void ProcessBulletDeath(int bulletId)
        {
            ServerBullet bullet = bullets.Find(b => b.id == bulletId);

            if (bullet != null)
            {
                Debug.Log("Remove bullet id:" + bulletId);
                bullets.Remove(bullet);
            }
        }
        #endregion
    }
}