using Game.Common.Requests;
using MultiplayerLibrary.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Common.Networking
{
    public static class GameMessageFormater
    {
        #region CONSTANTS
        private const int isReflectionMessageStart = 0;
        private const int sendTimeStart = sizeof(bool);
        private const int messageTypeStart = sendTimeStart + sizeof(int) * 5;
        private const int admissionTimeStart = messageTypeStart + sizeof(int);
        private const int messageIdStart = admissionTimeStart + sizeof(float);
        #endregion

        #region PUBLIC_METHODS
        public static GAME_MESSAGE_TYPE GetMessageType(byte[] data)
        {
            GAME_MESSAGE_TYPE messageType = (GAME_MESSAGE_TYPE)BitConverter.ToInt32(data, messageTypeStart);

            return messageType;
        }
        #endregion
    }
}