using MultiplayerLibrary.Entity;
using MultiplayerLibrary.Reflection.Attributes;
using MultiplayerLibrary.Reflection.Formater;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace MultiplayerLibrary.Reflection
{
    public class ReflectionHandler : MonoBehaviour
    {
        private BindingFlags InstanceDeclaredOnlyFilter => BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        public object entryPoint = null;
        private ClientNetworkManager clientNetwork;
        private Dictionary<string, object> savedVars = new Dictionary<string, object>();

        private object dicKey = null;

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

                Type type = entryPoint.GetType();
                OverWrite(data, entryPoint, type, name, name, ref offset);
            }
        }

        private void OverWrite(byte[] data, object obj, Type type, string path, string fullPath, ref int offset)
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

                            OverWrite(data, newObj, type, modifiedPath, fullPath, ref offset);
                        }
                        else
                        {
                            if (typeof(IEnumerable).IsAssignableFrom(newObj.GetType()))
                            {
                                bool isDicKey = pathParts[0].Contains(field.Name + "Key[");
                                bool isDicValue = pathParts[0].Contains(field.Name + "Value[");
                                int i = 0;

                                if (typeof(IDictionary).IsAssignableFrom(newObj.GetType()) && (isDicKey || isDicValue))
                                {
                                    int index = int.Parse(pathParts[0].Split('[')[1][0].ToString());
                                    object dicValue = null;
                                    object dicKey = null;
                                    bool found = false;

                                    foreach (DictionaryEntry entry in newObj as IDictionary)
                                    {
                                        if (i == index)
                                        {
                                            dicValue = (newObj as IDictionary)[entry.Key];
                                            dicKey = entry.Key;
                                            found = true;
                                            break;
                                        }
                                        else
                                        {
                                            i++;
                                        }
                                    }

                                    if (isDicValue)
                                    {
                                        if (found)
                                        {
                                            OverWriteVar(data, pathParts[0], ref dicValue, ref offset, fullPath, out bool success);

                                            if (success)
                                            {
                                                (newObj as IDictionary)[this.dicKey] = dicValue;
                                            }
                                        }
                                        else
                                        {
                                            (newObj as IDictionary).Add(this.dicKey, dicValue);
                                        }
                                    }
                                    else
                                    {
                                        this.dicKey = dicKey;//esto esta mal, hay que deserializar la key y despues guardarla y buscar el value
                                    }

                                    return;
                                }
                                else if (typeof(ICollection).IsAssignableFrom(newObj.GetType()) && pathParts[0].Contains(field.Name + "["))
                                {
                                    object collectionObj = null;

                                    foreach (object element in newObj as ICollection)
                                    {
                                        string fielName = field.Name + "[" + i.ToString() + "]";

                                        if (Equals(fielName, pathParts[0]))
                                        {
                                            collectionObj = element;
                                            break;
                                        }
                                        else
                                        {
                                            i++;
                                        }
                                    }

                                    OverWriteVar(data, pathParts[0], ref collectionObj, ref offset, fullPath, out bool success);
                                }
                            }
                            else
                            {
                                if (field.Name == pathParts[0])
                                {
                                    OverWriteVar(data, pathParts[0], ref newObj, ref offset, fullPath, out bool success);

                                    //object value = GetVarData(data, pathParts[0], newObj, ref offset);
                                    //field.SetValue(obj, value);
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

        //private void OverrideCollectionValue(ref object newObj, int i, object collectionValue)
        //{
        //    ICollection collection = newObj as ICollection;
        //
        //    Type type = collectionValue.GetType().GetGenericTypeDefinition();
        //
        //    if (type == typeof(IList))
        //    {
        //        (collection as IList)[i] = collectionValue;
        //    }
        //    else if (type == typeof(Queue<>))
        //    {
        //        Queue originalQueue = collection as Queue;
        //        Queue tempQueue = new Queue();
        //    
        //        for (int j = 0; j < originalQueue.Count; j++)
        //        {
        //            if (j == i)
        //            {
        //                tempQueue.Enqueue(collectionValue);
        //            }
        //            else
        //            {
        //                tempQueue.Enqueue(originalQueue.Dequeue());
        //            }
        //        }
        //    
        //        originalQueue = new Queue(tempQueue);
        //    }
        //    else if (type == typeof(Stack<>))
        //    {
        //        Stack originalStack = collection as Stack;
        //        Stack tempStack = new Stack();
        //    
        //        for (int j = 0; j < originalStack.Count; j++)
        //        {
        //            if (j == i)
        //            {
        //                tempStack.Push(collectionValue);
        //            }
        //            else
        //            {
        //                tempStack.Push(originalStack.Pop());
        //            }
        //        }
        //    
        //        originalStack = new Stack(tempStack);
        //    }
        //}

        private void OverWriteVar(byte[] data, string name, ref object obj, ref int offset, string fieldName, out bool success)
        {
            success = false;
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
                ((Transform)obj).Deserialize(name, data, ref offset, out success);
                value = obj;
            }
            else if (obj is byte[])
            {
                value = ((byte[])obj).Deserialize(name, data, ref offset, out success);
            }
            else
            {
                OverWrite(data, obj, obj.GetType(), name, fieldName, ref offset);
            }

            if (success)
            {
                obj = value;
                SaveIfChanged(value, fieldName);
            }
        }
        #endregion

        #region READ_METHODS
        private List<List<byte>> Inspect(object obj, Type type, string fieldName = "", bool valueVar = false)
        {
            List<List<byte>> output = new List<List<byte>>();

            foreach (FieldInfo field in type.GetFields(InstanceDeclaredOnlyFilter))
            {
                IEnumerable<Attribute> attributes = field.GetCustomAttributes();

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is SyncFieldAttribute)
                    {
                        Convert(output, field, obj, type, fieldName);
                    }
                }
            }

            if (valueVar)
            {
                ConvertValueVar(output, obj, fieldName);
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

        private void Convert(List<List<byte>> output, FieldInfo field, object obj, Type type, string fieldName = "")
        {
            object value = field.GetValue(obj);

            if (typeof(IEnumerable).IsAssignableFrom(value.GetType()))
            {
                if (typeof(IDictionary).IsAssignableFrom(value.GetType()))
                {
                    int i = 0;

                    foreach (DictionaryEntry entry in value as IDictionary)
                    {
                        ConvertToMessage(output, entry.Key, fieldName + field.Name + "Key[" + i.ToString() + "]");
                        ConvertToMessage(output, entry.Value, fieldName + field.Name + "Value[" + i.ToString() + "]", true);
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

        private void ConvertValueVar(List<List<byte>> output, object obj, string fieldName)
        {
            if (typeof(IEnumerable).IsAssignableFrom(obj.GetType()))
            {
                if (typeof(IDictionary).IsAssignableFrom(obj.GetType()))
                {
                    int i = 0;

                    foreach (DictionaryEntry entry in obj as IDictionary)
                    {
                        ConvertToMessage(output, entry.Key, fieldName + "Key[" + i.ToString() + "]");
                        ConvertToMessage(output, entry.Value, fieldName + "Value[" + i.ToString() + "]", true);
                        i++;
                    }
                }
                else if (typeof(ICollection).IsAssignableFrom(obj.GetType()))
                {
                    int i = 0;

                    foreach (object element in obj as ICollection)
                    {
                        ConvertToMessage(output, element, fieldName + "[" + i.ToString() + "]");
                        i++;
                    }
                }
            }
            else
            {
                ConvertToMessage(output, obj, fieldName);
            }
        }

        private void ConvertToMessage(List<List<byte>> msgStack, object obj, string fieldName, bool valueVar = false)
        {
            void SaveToSendIfChanged(object obj, List<byte> bytes)
            {
                if (SaveIfChanged(obj, fieldName))
                {
                    msgStack.Add(bytes);
                }
            }

            if (obj is int)
            {
                SaveToSendIfChanged(obj, ((int)obj).Serialize(fieldName));
            }
            else if (obj is float)
            {
                SaveToSendIfChanged(obj, ((float)obj).Serialize(fieldName));
            }
            else if (obj is bool)
            {
                SaveToSendIfChanged(obj, ((bool)obj).Serialize(fieldName));
            }
            else if (obj is char)
            {
                SaveToSendIfChanged(obj, ((char)obj).Serialize(fieldName));
            }
            else if (obj is string)
            {
                SaveToSendIfChanged(obj, ((string)obj).Serialize(fieldName));
            }
            else if (obj is Vector2)
            {
                SaveToSendIfChanged(obj, ((Vector2)obj).Serialize(fieldName));
            }
            else if (obj is Vector3)
            {
                SaveToSendIfChanged(obj, ((Vector3)obj).Serialize(fieldName));
            }
            else if (obj is Quaternion)
            {
                SaveToSendIfChanged(obj, ((Quaternion)obj).Serialize(fieldName));
            }
            else if (obj is Color)
            {
                SaveToSendIfChanged(obj, ((Color)obj).Serialize(fieldName));
            }
            else if (obj is Transform)
            {
                SaveToSendIfChanged(obj, ((Transform)obj).Serialize(fieldName));
            }
            else if (obj is byte[])
            {
                msgStack.Add(((byte[])obj).Serialize(fieldName));
            }
            else
            {
                foreach (List<byte> msg in Inspect(obj, obj.GetType(), fieldName + "\\", valueVar))
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
                    //Debug.Log(method.Name);
                    method.Invoke(obj, new object[] { dataBytes });
                }
            }
        }
        #endregion

        #region PRIVATE_METHODS
        private bool SaveIfChanged(object value, string fullPath)
        {
            if (savedVars.ContainsKey(fullPath))
            {
                if (!Equals(savedVars[fullPath], value))
                {
                    savedVars[fullPath] = value;
                    return true;
                }
            }
            else
            {
                savedVars.Add(fullPath, value);
                return true;
            }

            return true;
        }
        #endregion
    }
}