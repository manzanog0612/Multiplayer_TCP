using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerLibrary.Reflection
{
    public static class NetSerialization
    {
        #region PUBLIC_FIELDS
        public static List<byte> Serialize(this int intValue, string fieldName)
        {
            Debug.Log(intValue);

            List<byte> bytes = SerializeMessageName(fieldName);
            bytes.AddRange(BitConverter.GetBytes(intValue).ToList());
            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), intValue));

            return bytes;
        }

        public static List<byte> Serialize(this float floatValue, string fieldName)
        {
            Debug.Log(floatValue);

            List<byte> bytes = SerializeMessageName(fieldName);
            bytes.AddRange(BitConverter.GetBytes(floatValue).ToList());
            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), (int)(floatValue * 100f)));

            return bytes;
        }

        public static List<byte> Serialize(this bool boolValue, string fieldName)
        {
            Debug.Log(boolValue);

            List<byte> bytes = SerializeMessageName(fieldName);
            bytes.AddRange(BitConverter.GetBytes(boolValue).ToList());
            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), boolValue ? 1 : 0));

            return bytes;
        }

        public static List<byte> Serialize(this char charValue, string fieldName)
        {
            Debug.Log(charValue);

            List<byte> bytes = SerializeMessageName(fieldName);
            bytes.AddRange(BitConverter.GetBytes(charValue).ToList());
            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), charValue));

            return bytes;
        }

        public static List<byte> Serialize(this string stringValue, string fieldName)
        {
            Debug.Log(stringValue);

            List<byte> bytes = SerializeMessageName(fieldName);

            int operation = 0;

            bytes.AddRange(BitConverter.GetBytes(stringValue.Length));

            for (int i = 0; i < stringValue.Length; i++)
            {
                bytes.AddRange(BitConverter.GetBytes(stringValue[i]));
                operation += stringValue[i];
            }

            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), operation));

            return bytes;
        }

        public static List<byte> Serialize(this Vector2 vector2Value, string fieldName)
        {
            Debug.Log(vector2Value);

            List<byte> bytes = SerializeMessageName(fieldName);
            bytes.AddRange(BitConverter.GetBytes(vector2Value.x).ToList());
            bytes.AddRange(BitConverter.GetBytes(vector2Value.y).ToList());

            int operation = (int)(vector2Value.x * 100) + (int)(vector2Value.y * 100);

            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), operation));

            return bytes;
        }

        public static List<byte> Serialize(this Vector3 vector3Value, string fieldName)
        {
            Debug.Log(vector3Value);

            List<byte> bytes = SerializeMessageName(fieldName);
            bytes.AddRange(BitConverter.GetBytes(vector3Value.x).ToList());
            bytes.AddRange(BitConverter.GetBytes(vector3Value.y).ToList());
            bytes.AddRange(BitConverter.GetBytes(vector3Value.z).ToList());

            int operation = (int)(vector3Value.x * 100) + (int)(vector3Value.y * 100) + (int)(vector3Value.z * 100);

            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), operation));

            return bytes;
        }

        public static List<byte> Serialize(this Quaternion quaternionValue, string fieldName)
        {
            Debug.Log(quaternionValue);

            List<byte> bytes = SerializeMessageName(fieldName);
            bytes.AddRange(BitConverter.GetBytes(quaternionValue.x).ToList());
            bytes.AddRange(BitConverter.GetBytes(quaternionValue.y).ToList());
            bytes.AddRange(BitConverter.GetBytes(quaternionValue.z).ToList());
            bytes.AddRange(BitConverter.GetBytes(quaternionValue.w).ToList());

            int operation = (int)(quaternionValue.x * 100) + (int)(quaternionValue.y * 100) + (int)(quaternionValue.z * 100) + (int)(quaternionValue.w * 100);

            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), operation));

            return bytes;
        }

        public static List<byte> Serialize(this Color colorValue, string fieldName)
        {
            Debug.Log(colorValue);

            List<byte> bytes = SerializeMessageName(fieldName);
            bytes.AddRange(BitConverter.GetBytes(colorValue.r).ToList());
            bytes.AddRange(BitConverter.GetBytes(colorValue.g).ToList());
            bytes.AddRange(BitConverter.GetBytes(colorValue.b).ToList());
            bytes.AddRange(BitConverter.GetBytes(colorValue.a).ToList());

            int operation = (int)(colorValue.r * 100) + (int)(colorValue.g * 100) + (int)(colorValue.b * 100) + (int)(colorValue.a * 100);

            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), operation));

            return bytes;
        }

        public static List<byte> Serialize(this Transform transformValue, string fieldName)
        {
            Debug.Log(transformValue);
            
            List<byte> bytes = SerializeMessageName(fieldName);
            bytes.AddRange(BitConverter.GetBytes(transformValue.position.x).ToList());
            bytes.AddRange(BitConverter.GetBytes(transformValue.position.y).ToList());
            bytes.AddRange(BitConverter.GetBytes(transformValue.position.z).ToList());
            bytes.AddRange(BitConverter.GetBytes(transformValue.rotation.x).ToList());
            bytes.AddRange(BitConverter.GetBytes(transformValue.rotation.y).ToList());
            bytes.AddRange(BitConverter.GetBytes(transformValue.rotation.z).ToList());
            bytes.AddRange(BitConverter.GetBytes(transformValue.rotation.w).ToList());
            bytes.AddRange(BitConverter.GetBytes(transformValue.localScale.x).ToList());
            bytes.AddRange(BitConverter.GetBytes(transformValue.localScale.y).ToList());
            bytes.AddRange(BitConverter.GetBytes(transformValue.localScale.z).ToList());

            int operation = (int)(transformValue.position.x * 100) + (int)(transformValue.position.y * 100) + (int)(transformValue.position.z * 100) + 
                            (int)(transformValue.rotation.x * 100) + (int)(transformValue.rotation.y * 100) + (int)(transformValue.rotation.z * 100) + (int)(transformValue.rotation.w * 100) +
                            (int)(transformValue.localScale.x * 100) + (int)(transformValue.localScale.y * 100) + (int)(transformValue.localScale.z * 100);
            
            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), operation));
            
            return bytes;
        }

        public static List<byte> Serialize(this byte[] byteArrayValue, string fieldName)
        {
            List<byte> bytes = SerializeMessageName(fieldName);
            bytes.AddRange(BitConverter.GetBytes(byteArrayValue.Length));
            bytes.AddRange(byteArrayValue);

            int operation = 0;

            for (int i = 0; i < byteArrayValue.Length; i++)
            {
                operation += byteArrayValue[i];
            }

            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), operation));

            return bytes;
        }
        #endregion

        #region PRIVATE_FIELDS
        private static List<byte> SerializeMessageName(string fieldName)
        {
            List<byte> bytes = new List<byte>();

            int fieldnameSize = fieldName.Length;

            bytes.AddRange(BitConverter.GetBytes(fieldnameSize));

            for (int i = 0; i < fieldnameSize; i++)
            {
                bytes.AddRange(BitConverter.GetBytes(fieldName[i]));
            }

            return bytes;
        }

        private static List<byte> SerializeMessageTail(byte[] messageBytes, int operation)
        {
            List<byte> bytes = new List<byte>();

            int messageSize = messageBytes.Length;

            bytes.AddRange(BitConverter.GetBytes(operation));
            bytes.AddRange(BitConverter.GetBytes(messageSize + sizeof(int) * 2));

            return bytes;
        }
        #endregion
    }
}
