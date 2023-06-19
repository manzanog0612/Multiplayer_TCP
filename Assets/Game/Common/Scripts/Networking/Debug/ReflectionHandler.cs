using MultiplayerLibrary.Entity;
using MultiplayerLibrary.Reflection.Attributes;
using MultiplayerLibrary.Reflection.Formater;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;

namespace MultiplayerLibrary.Reflection
{
    public class ReflectionHandler : MonoBehaviour
    {
        private BindingFlags InstanceDeclaredOnlyFilter => BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        public object entryPoint = null;
        private ClientNetworkManager clientNetwork;

        #region UNITY_CALLS
        public void Start()
        {
            NetworkManager.Instance.onReceiveReflectionData += StartToOverWrite;

            clientNetwork = NetworkManager.Instance as ClientNetworkManager;
        }

        public void Update()
        {
            if (entryPoint == null)
            {
                return;
            } 

            Type type = entryPoint.GetType();
            List<List<byte>> gameManagerAsBytes = Inspect(entryPoint, type);

            List<byte> dataBytes = new List<byte>();

            dataBytes.AddRange(BitConverter.GetBytes(true).ToList()); // isReflectionMessage
            dataBytes.AddRange(BitConverter.GetBytes(clientNetwork.assignedId).ToList()); // clientId
            dataBytes.AddRange(BitConverter.GetBytes(gameManagerAsBytes.Count).ToList()); // datasAmount

            foreach (List<byte> dataItem in gameManagerAsBytes)
            {
                dataBytes.AddRange(dataItem);
            }

            CallSyncMethods(clientNetwork, dataBytes.ToArray());
        }
        #endregion

        #region PUBLIC_METHODS
        public void SetEntryPoint(object var)
        {
            entryPoint = var;
        }
        #endregion

        #region OVERWRITE_METHODS
        private void StartToOverWrite(byte[] data)
        {
            int clientId = ReflectionMessageFormater.GetClientId(data);

            if (clientNetwork.assignedId == clientId)
            {
                return;
            }

            int datasAmount = ReflectionMessageFormater.GetDatasAmount(data);

            int offset = sizeof(bool) + sizeof(int) * 2;

            for (int i = 0; i < datasAmount; i++)
            {
                int sizeOfName = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);

                string name = string.Empty;

                for (int j = 0; j < sizeOfName * sizeof(char); j += sizeof(char))
                {
                    char c = BitConverter.ToChar(data, offset);
                    name += c;
                    offset += sizeof(char);
                }

                Debug.Log(name);

                Type type = entryPoint.GetType();
                OverWrite(data, entryPoint, type, name, ref offset);
            }
        }

        private void OverWrite(byte[] data, object obj, Type type, string path, ref int offset)
        {
            string[] pathParts = path.Split("\\");

            foreach (FieldInfo field in type.GetFields(InstanceDeclaredOnlyFilter))
            {
                IEnumerable<Attribute> attributes = field.GetCustomAttributes();

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is SyncFieldAttribute)
                    {
                        object newObj = field.GetValue(obj);

                        if (pathParts.Length > 1)
                        {
                            string modifiedPath = string.Empty;

                            for (int i = 0; i < pathParts[0].Length; i++)
                            {
                                offset += sizeof(char);
                            }

                            for (int i = 1; i < pathParts.Length; i++)
                            {
                                modifiedPath += pathParts[i];
                            }

                            OverWrite(data, newObj, type, modifiedPath, ref offset);
                        }
                        else
                        {
                            if (typeof(IEnumerable).IsAssignableFrom(newObj.GetType()))
                            {
                                int offsetAux = offset;

                                if (typeof(IDictionary).IsAssignableFrom(newObj.GetType()) && pathParts[0].Contains(field.Name + "Key["))
                                {
                                    object dicKey = null;
                                    object dicValue = null;

                                    bool found = false;

                                    foreach (DictionaryEntry entry in newObj as IDictionary)
                                    {
                                        dicKey = GetVarData(data, pathParts[0], entry.Key, ref offsetAux);

                                        if (Equals(dicKey, entry.Key))
                                        {
                                            offset = offsetAux;

                                            string dicValueFieldName = string.Empty.Deserialize(string.Empty, data, ref offsetAux, out bool success);
                                            offset += dicValueFieldName.Length * sizeof(char) + sizeof(int);

                                            dicValue = GetVarData(data, dicValueFieldName, entry.Value, ref offset);
                                            found = true;

                                            break;
                                        }
                                        else
                                        {
                                            offsetAux = offset;
                                        }
                                    }

                                    if (found)
                                    {
                                        (newObj as IDictionary)[dicKey] = dicValue;
                                    }

                                    return;
                                }
                                else if (typeof(ICollection).IsAssignableFrom(newObj.GetType()) && pathParts[0].Contains(field.Name + "["))
                                {
                                    int i = 0;
                                    foreach (object element in newObj as ICollection)
                                    {
                                        string fielName = field.Name + "[" + i.ToString() + "]";

                                        if (Equals(fielName, pathParts[0]))
                                        {
                                            offset = offsetAux;

                                            object collectionValue = GetVarData(data, pathParts[0], element, ref offset);

                                            OverrideCollectionValue(ref newObj, i, collectionValue);

                                            break;
                                        }
                                        else
                                        {
                                            offsetAux = offset;
                                        }

                                        i++;
                                    }
                                }
                            }
                            else
                            {
                                if (field.Name == pathParts[0])
                                {
                                    object value = GetVarData(data, pathParts[0], newObj, ref offset);
                                    field.SetValue(obj, value);
                                }
                            }
                        }
                    }
                }
            }

            if (offset == data.Length)
            {
                return;
            }

            if (typeof(ISync).IsAssignableFrom(type))
            {
                byte[] value = new byte[0].Deserialize(path, data, ref offset, out bool success);
                (obj as ISync).Deserialize(value);
                return;
            }
        }

        private void OverrideCollectionValue(ref object newObj, int i, object collectionValue)
        {
            ICollection collection = newObj as ICollection;

            Type type = collectionValue.GetType().GetGenericTypeDefinition();

            if (type == typeof(IList))
            {
                (collection as IList)[i] = collectionValue;
            }
            else if (type == typeof(Queue<>))
            {
                Queue originalQueue = collection as Queue;
                Queue tempQueue = new Queue();
            
                for (int j = 0; j < originalQueue.Count; j++)
                {
                    if (j == i)
                    {
                        tempQueue.Enqueue(collectionValue);
                    }
                    else
                    {
                        tempQueue.Enqueue(originalQueue.Dequeue());
                    }
                }
            
                originalQueue = new Queue(tempQueue);
            }
            else if (type == typeof(Stack<>))
            {
                Stack originalStack = collection as Stack;
                Stack tempStack = new Stack();
            
                for (int j = 0; j < originalStack.Count; j++)
                {
                    if (j == i)
                    {
                        tempStack.Push(collectionValue);
                    }
                    else
                    {
                        tempStack.Push(originalStack.Pop());
                    }
                }
            
                originalStack = new Stack(tempStack);
            }
        }

        private object GetVarData(byte[] data, string name, object obj, ref int offset)
        {
            bool success = false;
            object value = null;

            if (obj is int)
            {
                value = ((int)obj).Deserialize(name, data, ref offset, out success);
            }
            else if (obj is float)
            {
                value = ((float)obj).Deserialize(name, data, ref offset, out success);
            }
            else if (obj is bool)
            {
                value = ((bool)obj).Deserialize(name, data, ref offset, out success);
            }
            else if (obj is char)
            {
                value = ((char)obj).Deserialize(name, data, ref offset, out success);
            }
            else if (obj is string)
            {
                value = ((string)obj).Deserialize(name, data, ref offset, out success);
            }
            else if (obj is Vector2)
            {
                value = ((Vector2)obj).Deserialize(name, data, ref offset, out success);
            }
            else if (obj is Vector3)
            {
                value = ((Vector3)obj).Deserialize(name, data, ref offset, out success);
            }
            else if (obj is Quaternion)
            {
                value = ((Quaternion)obj).Deserialize(name, data, ref offset, out success);
            }
            else if (obj is Color)
            {
                value = ((Color)obj).Deserialize(name, data, ref offset, out success);
            }
            else if (obj is Transform)
            {
                value = ((Transform)obj).Deserialize(name, data, ref offset, out success);
            }
            else if (obj is byte[])
            {
                value = ((byte[])obj).Deserialize(name, data, ref offset, out success);
            }
            else
            {
                OverWrite(data, obj, obj.GetType(), name, ref offset);
            }

            return success ? value : null;
        }
        #endregion

        #region READ_METHODS
        private List<List<byte>> Inspect(object obj, Type type, string fieldName = "")
        {
            List<List<byte>> output = new List<List<byte>>();

            foreach (FieldInfo field in type.GetFields(InstanceDeclaredOnlyFilter))
            {
                IEnumerable<Attribute> attributes = field.GetCustomAttributes();

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is SyncFieldAttribute)
                    {
                        object value = field.GetValue(obj);

                        if (typeof(IEnumerable).IsAssignableFrom(value.GetType()))
                        {
                            if (typeof(IDictionary).IsAssignableFrom(value.GetType()))
                            {
                                int i = 0;

                                foreach (DictionaryEntry entry in value as IDictionary)
                                {
                                    List<byte> dicList = new List<byte>();
                                    dicList.AddRange(ConvertToMessage(entry.Key, fieldName + field.Name + "Key[" + i.ToString() + "]"));
                                    dicList.AddRange(ConvertToMessage(entry.Value, "Value[" + i.ToString() + "]"));
                                    output.Add(dicList);
                                    i++;
                                }
                            }
                            else if (typeof(ICollection).IsAssignableFrom(value.GetType()))
                            {
                                int i = 0;

                                foreach (object element in value as ICollection)
                                {
                                    ConvertToMessage(output, element, fieldName + field.Name + "[" + i.ToString() + "]");
                                    i++;
                                }
                            }
                        }
                        else
                        {
                            ConvertToMessage(output, value, fieldName + field.Name);
                        }
                    }
                }
            }

            if (typeof(ISync).IsAssignableFrom(type))
            {
                ISync a = (obj as ISync);
                byte[] bytes = a.Serialize();
                ConvertToMessage(output, bytes, fieldName);
            }

            if (type.BaseType != null)
            {
                foreach (List<byte> msg in Inspect(obj, type.BaseType, fieldName))
                {
                    output.Add(msg);
                }
            }

            return output;
        }

        private void ConvertToMessage(List<List<byte>> msgStack, object obj, string fieldName)
        {
            if (obj is int)
            {
                msgStack.Add(((int)obj).Serialize(fieldName));
            }
            else if (obj is float)
            {
                msgStack.Add(((float)obj).Serialize(fieldName));
            }
            else if (obj is bool)
            {
                msgStack.Add(((bool)obj).Serialize(fieldName));
            }
            else if (obj is char)
            {
                msgStack.Add(((char)obj).Serialize(fieldName));
            }
            else if (obj is string)
            {
                msgStack.Add(((string)obj).Serialize(fieldName));
            }
            else if (obj is Vector2)
            {
                msgStack.Add(((Vector2)obj).Serialize(fieldName));
            }
            else if (obj is Vector3)
            {
                msgStack.Add(((Vector3)obj).Serialize(fieldName));
            }
            else if (obj is Quaternion)
            {
                msgStack.Add(((Quaternion)obj).Serialize(fieldName));
            }
            else if (obj is Color)
            {
                msgStack.Add(((Color)obj).Serialize(fieldName));
            }
            else if (obj is Transform)
            {
                msgStack.Add(((Transform)obj).Serialize(fieldName));
            }
            else if (obj is byte[])
            {
                msgStack.Add(((byte[])obj).Serialize(fieldName));
            }
            else
            {
                foreach (List<byte> msg in Inspect(obj, obj.GetType(), fieldName + "\\"))
                {
                    msgStack.Add(msg);
                }
            }
        }

        private void CallSyncMethods(object obj, byte[] dataBytes)
        {
            foreach (MethodInfo method in obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                SyncMethodAttribute attribute = method.GetCustomAttribute<SyncMethodAttribute>();

                if (attribute != null)
                {
                    Debug.Log(method.Name);
                    method.Invoke(obj, new object[] { dataBytes });
                }
            }
        }

        private List<byte> ConvertToMessage(object obj, string fieldName)
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
            else if (obj is byte[])
            {
                return ((byte[])obj).Serialize(fieldName);
            }
            else
            {
                List<byte> bytes = new List<byte>();
                foreach (List<byte> msg in Inspect(obj, obj.GetType(), fieldName + "\\"))
                {
                    bytes.AddRange(msg);
                }

                return bytes;
            }
        }
        #endregion
    }
}

