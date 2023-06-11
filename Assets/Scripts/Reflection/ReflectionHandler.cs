using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ReflectionHandler : MonoBehaviour
{
    private BindingFlags InstanceDeclaredOnlyFilter => BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    public GameManager gameManager = null;
    public ClientHandler clientHandler = null;

    #region UNITY_CALLS
    void Start()
    {
        NetworkManager.Instance.onReceiveReflectionData += StartToOverWrite;
    }

    void Update()
    {
        if (!clientHandler.Initialized)
        {
            return;
        }

        List<List<byte>> gameManagerAsBytes = Inspect(gameManager, typeof(GameManager));

        List<byte> dataBytes = new List<byte>();

        dataBytes.AddRange(BitConverter.GetBytes(true).ToList()); // isReflectionMessage
        dataBytes.AddRange(BitConverter.GetBytes(clientHandler.ClientNetworkManager.assignedId).ToList()); // clientId
        dataBytes.AddRange(BitConverter.GetBytes(gameManagerAsBytes.Count).ToList()); // datasAmount

        foreach (List<byte> dataItem in gameManagerAsBytes)
        {
            dataBytes.AddRange(dataItem);
        }

        CallSyncMethods(clientHandler.ClientNetworkManager, dataBytes.ToArray());
    }
    #endregion

    #region OVERWRITE_METHODS
    private void StartToOverWrite(byte[] data)
    {
        int clientId = ReflectionMessageFormater.GetClientId(data);
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

            OverWrite(data, gameManager, typeof(GameManager), name, ref offset);
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
                                    dicKey = GetVarData(data, entry.Key, ref offsetAux);

                                    if (Equals(dicKey, entry.Key))
                                    {
                                        offset = offsetAux;

                                        object dicValueFieldName = GetVarData(data, string.Empty, ref offset);

                                        dicValue = GetVarData(data, entry.Value, ref offset);

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
                                else
                                {
                                    (newObj as IDictionary).Add(dicKey, dicValue);
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

                                        object collectionValue = GetVarData(data, element, ref offset);

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
                                object value = GetVarData(data, newObj, ref offset);
                                field.SetValue(obj, value);
                            }
                        }
                    }
                }
            }
        }
    }

    /*private void OverWriteDeCave(object obj, Type type, string receivedFieldName, object newValue, string fieldName = "")
    {
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
                            int iToModify = -1;

                            foreach (DictionaryEntry entry in value as IDictionary)
                            {
                                fieldName += fieldName + "Value[" + i.ToString() + "]";
                                OverWriteVar(data, entry.Key, fieldName + field.Name + "Key[" + i.ToString() + "]");
                                OverWriteVar(data, entry.Value, fieldName + field.Name + "Value[" + i.ToString() + "]");
                                i++;
                            }
                        }
                        else if (typeof(ICollection).IsAssignableFrom(value.GetType()))
                        {
                            int i = 0;

                            foreach (object element in value as ICollection)
                            {
                                OverWriteVar(output, element, fieldName + field.Name + "[" + i.ToString() + "]");
                                i++;
                            }
                        }
                    }
                    else
                    {
                        field.SetValue(obj, );
                        OverWriteVar(output, value, fieldName + field.Name, offset);
                    }
                }
            }
        }
    }*/

    private void OverrideCollectionValue(ref object newObj, int i, object collectionValue)
    {
        ICollection collection = newObj as ICollection;

        Type type = collectionValue.GetType();

        if (collection is IList)
        {
            (collection as IList)[i] = 'l';
        }
        //else if (collection is Queue<type>)
        //{
        //    Queue originalQueue = collection as Queue;
        //    Queue tempQueue = new Queue();
        //
        //    for (int j = 0; j < originalQueue.Count; j++)
        //    {
        //        if (j == i)
        //        {
        //            tempQueue.Enqueue('g');
        //        }
        //        else
        //        {
        //            tempQueue.Enqueue(originalQueue.Dequeue());
        //        }
        //    }
        //
        //    originalQueue = new Queue(tempQueue);
        //}
        //else if (collection is Stack)
        //{
        //    Stack originalStack = collection as Stack;
        //    Stack tempStack = new Stack();
        //
        //    for (int j = 0; j < originalStack.Count; j++)
        //    {
        //        if (j == i)
        //        {
        //            tempStack.Push('g');
        //        }
        //        else
        //        {
        //            tempStack.Push(originalStack.Pop());
        //        }
        //    }
        //
        //    originalStack = new Stack(tempStack);
        //}
    }

    private object GetVarData(byte[] data, object obj, ref int offset)
    {
        if (obj is int)
        {
            return ((int)obj).Deserialize(data, ref offset);
        }
        else if (obj is float)
        {
            return ((float)obj).Deserialize(data, ref offset);
        }
        else if (obj is bool)
        {
            return ((bool)obj).Deserialize(data, ref offset);
        }
        else if (obj is char)
        {
            return ((char)obj).Deserialize(data, ref offset);
        }
        else if (obj is string)
        {
            return ((string)obj).Deserialize(data, ref offset);
        }
        else if (obj is Vector3)
        {
            return ((Vector3)obj).Deserialize(data, ref offset);
        }
        else
        {
            return null;
        }
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
        else if (obj is Vector3)
        {
            msgStack.Add(((Vector3)obj).Serialize(fieldName));
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
        foreach (MethodInfo method in obj.GetType().GetMethods(InstanceDeclaredOnlyFilter))
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
        else if (obj is Vector3)
        {
            return ((Vector3)obj).Serialize(fieldName);
        }
        else
        {
            Debug.LogError("No type to convert");
            return null;
        }
    }
    #endregion
}

