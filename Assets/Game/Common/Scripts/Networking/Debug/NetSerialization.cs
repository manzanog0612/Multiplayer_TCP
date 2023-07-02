using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerLibrary.Reflection
{
   
    public static class NetSerialization
    {
        
        #region PUBLIC_FIELDS
        public static List<byte> Serialize(object obj, string fieldName)
        {
            if (obj is int)
            {
                return ((int)obj).Serialize(fieldName);
            }
            else if (obj is float)
            {
                return ((float)obj).Serialize(fieldName);
            }
            else if (obj is bool)
            {
                return ((bool)obj).Serialize(fieldName);
            }
            else if (obj is char)
            {
                return ((char)obj).Serialize(fieldName);
            }
            else if (obj is string)
            {
                return ((string)obj).Serialize(fieldName);
            }
            else if (obj is Vector2)
            {
                return ((Vector2)obj).Serialize(fieldName);
            }
            else if (obj is Vector3)
            {
                return ((Vector3)obj).Serialize(fieldName);
            }
            else if (obj is Quaternion)
            {
                return ((Quaternion)obj).Serialize(fieldName);
            }
            else if (obj is Color)
            {
                return ((Color)obj).Serialize(fieldName);
            }
            else if (obj is Transform)
            {
                return ((Transform)obj).Serialize(fieldName);
            }
            //else if (obj is IEnumerator)
            //{
            //    if (obj is IDictionary)
            //    { 
            //        return ((IDictionary)obj).Serialize(fieldName); 
            //    }
            //    else
            //    {
            //        TYPE type = TYPE.ILIST;
            //
            //        Type objType = ((ICollection)obj).GetType().GetGenericTypeDefinition();
            //
            //        if (objType == typeof(Queue<>))
            //        {
            //            type = TYPE.QUEUE;
            //        }
            //        else if (objType == typeof(Stack<>))
            //        {
            //            type = TYPE.STACK;
            //        }
            //
            //        return ((ICollection)obj).Serialize(type, fieldName);
            //    }
            //}
            else if (obj is byte[])
            {
                return ((byte[])obj).Serialize(fieldName);
            }
            else
            {
                Debug.Log("type not supported");
                return null;
            }
        }
        #endregion

        #region SERIALIZE_METHODS
        private static List<byte> Serialize(this int intValue, string fieldName)
        {
            Debug.Log(intValue);

            List<byte> bytes = SerializeMessageHeader(fieldName);
            bytes.AddRange(BitConverter.GetBytes(intValue));
            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), intValue));

            return bytes;
        }
        private static List<byte> Serialize(this float floatValue, string fieldName)
        {
            Debug.Log(floatValue);

            List<byte> bytes = SerializeMessageHeader(fieldName);
            bytes.AddRange(BitConverter.GetBytes(floatValue));
            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), (int)(floatValue * 100f)));

            return bytes;
        }
        private static List<byte> Serialize(this bool boolValue, string fieldName)
        {
            Debug.Log(boolValue);

            List<byte> bytes = SerializeMessageHeader(fieldName);
            bytes.AddRange(BitConverter.GetBytes(boolValue));
            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), boolValue ? 1 : 0));

            return bytes;
        }
        private static List<byte> Serialize(this char charValue, string fieldName)
        {
            Debug.Log(charValue);

            List<byte> bytes = SerializeMessageHeader(fieldName);
            bytes.AddRange(BitConverter.GetBytes(charValue));
            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), charValue));

            return bytes;
        }
        private static List<byte> Serialize(this string stringValue, string fieldName)
        {
            Debug.Log(stringValue);

            List<byte> bytes = SerializeMessageHeader(fieldName);

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
        private static List<byte> Serialize(this Vector2 vector2Value, string fieldName)
        {
            Debug.Log(vector2Value);

            List<byte> bytes = SerializeMessageHeader(fieldName);
            bytes.AddRange(BitConverter.GetBytes(vector2Value.x));
            bytes.AddRange(BitConverter.GetBytes(vector2Value.y));

            int operation = (int)(vector2Value.x * 100) + (int)(vector2Value.y * 100);

            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), operation));

            return bytes;
        }
        private static List<byte> Serialize(this Vector3 vector3Value, string fieldName)
        {
            Debug.Log(vector3Value);

            List<byte> bytes = SerializeMessageHeader(fieldName);
            bytes.AddRange(BitConverter.GetBytes(vector3Value.x));
            bytes.AddRange(BitConverter.GetBytes(vector3Value.y));
            bytes.AddRange(BitConverter.GetBytes(vector3Value.z));

            int operation = (int)(vector3Value.x * 100) + (int)(vector3Value.y * 100) + (int)(vector3Value.z * 100);

            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), operation));

            return bytes;
        }
        private static List<byte> Serialize(this Quaternion quaternionValue, string fieldName)
        {
            Debug.Log(quaternionValue);

            List<byte> bytes = SerializeMessageHeader(fieldName);
            bytes.AddRange(BitConverter.GetBytes(quaternionValue.x));
            bytes.AddRange(BitConverter.GetBytes(quaternionValue.y));
            bytes.AddRange(BitConverter.GetBytes(quaternionValue.z));
            bytes.AddRange(BitConverter.GetBytes(quaternionValue.w));

            int operation = (int)(quaternionValue.x * 100) + (int)(quaternionValue.y * 100) + (int)(quaternionValue.z * 100) + (int)(quaternionValue.w * 100);

            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), operation));

            return bytes;
        }
        private static List<byte> Serialize(this Color colorValue, string fieldName)
        {
            Debug.Log(colorValue);

            List<byte> bytes = SerializeMessageHeader(fieldName);
            bytes.AddRange(BitConverter.GetBytes(colorValue.r));
            bytes.AddRange(BitConverter.GetBytes(colorValue.g));
            bytes.AddRange(BitConverter.GetBytes(colorValue.b));
            bytes.AddRange(BitConverter.GetBytes(colorValue.a));

            int operation = (int)(colorValue.r * 100) + (int)(colorValue.g * 100) + (int)(colorValue.b * 100) + (int)(colorValue.a * 100);

            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), operation));

            return bytes;
        }
        private static List<byte> Serialize(this Transform transformValue, string fieldName)
        {
            Debug.Log(transformValue);
            
            List<byte> bytes = SerializeMessageHeader(fieldName);
            bytes.AddRange(BitConverter.GetBytes(transformValue.position.x));
            bytes.AddRange(BitConverter.GetBytes(transformValue.position.y));
            bytes.AddRange(BitConverter.GetBytes(transformValue.position.z));
            bytes.AddRange(BitConverter.GetBytes(transformValue.rotation.x));
            bytes.AddRange(BitConverter.GetBytes(transformValue.rotation.y));
            bytes.AddRange(BitConverter.GetBytes(transformValue.rotation.z));
            bytes.AddRange(BitConverter.GetBytes(transformValue.rotation.w));
            bytes.AddRange(BitConverter.GetBytes(transformValue.localScale.x));
            bytes.AddRange(BitConverter.GetBytes(transformValue.localScale.y));
            bytes.AddRange(BitConverter.GetBytes(transformValue.localScale.z));

            int operation = (int)(transformValue.position.x * 100) + (int)(transformValue.position.y * 100) + (int)(transformValue.position.z * 100) + 
                            (int)(transformValue.rotation.x * 100) + (int)(transformValue.rotation.y * 100) + (int)(transformValue.rotation.z * 100) + (int)(transformValue.rotation.w * 100) +
                            (int)(transformValue.localScale.x * 100) + (int)(transformValue.localScale.y * 100) + (int)(transformValue.localScale.z * 100);
            
            bytes.AddRange(SerializeMessageTail(bytes.ToArray(), operation));
            return bytes;
        }
        private static List<byte> Serialize(this IDictionary dictionaryValue, string fieldName)
        {
            List<byte> bytes = SerializeMessageHeader(fieldName);

            bytes.AddRange(BitConverter.GetBytes(dictionaryValue.Count));

            foreach (DictionaryEntry kvp in dictionaryValue)
            {
                List<byte> serializedKey = Serialize(kvp.Key, fieldName);
                bytes.AddRange(serializedKey);

                List<byte> serializedValue = Serialize(kvp.Key, fieldName);
                bytes.AddRange(serializedValue);
            }

            return bytes;
        }
        private static List<byte> Serialize(this ICollection collectionValue, string fieldName)
        {
            List<byte> bytes = SerializeMessageHeader(fieldName);

            bytes.AddRange(BitConverter.GetBytes(collectionValue.Count));

            foreach (var item in collectionValue)
            {
                List<byte> serializedKey = Serialize(item, fieldName);
                bytes.AddRange(serializedKey);
            }

            return bytes;
        }
        private static List<byte> Serialize(this byte[] byteArrayValue, string fieldName)
        {
            List<byte> bytes = SerializeMessageHeader(fieldName);
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
        private static List<byte> SerializeMessageHeader(string fieldName)
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
