using MultiplayerLibrary.Entity;
using MultiplayerLibrary.Reflection.Attributes;
using MultiplayerLibrary.Reflection.Formater;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        private Queue<object> dicKeys = new Queue<object>();
        private string actualUsingKeyName = string.Empty;
        private string actualUsingValueName = string.Empty;

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
            List<List<byte>> gameManagerAsBytes = Inspect(entryPoint, type, false);

            if (gameManagerAsBytes.Count == 0)
            {
                return;
            }

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

                //Debug.Log(name);

                Type type = entryPoint.GetType();
                OverWrite(data, entryPoint, type, name, ref offset);
            }
        }

        private object OverWrite(byte[] data, object obj, Type type, string fullPath, ref int offset, string path = "", bool isObjectValue = false)
        {
            foreach (FieldInfo field in type.GetFields(InstanceDeclaredOnlyFilter))
            {
                IEnumerable<Attribute> attributes = field.GetCustomAttributes();

                foreach (Attribute attribute in attributes)
                {
                    if (attribute is SyncFieldAttribute)
                    {
                        object newObj = field.GetValue(obj);

                        OverWriteValue(data, newObj, fullPath, ref offset, path + field.Name, isObjectValue);
                    }
                }
            }

            if (isObjectValue)
            {
                OverWriteValue(data, obj, fullPath, ref offset, path, isObjectValue);
            }

            if (typeof(ISync).IsAssignableFrom(type))
            {
                byte[] value = NetDeserialization.DeserializeBytes(path, data, ref offset, out bool success);

                if (success)
                { 
                    (obj as ISync).Deserialize(value); 
                }
                return null;
            }

            return null;
        }

        private void OverWriteValue(byte[] data, object newObj, string fullPath, ref int offset, string path, bool isObjectValue)
        {
            if (typeof(IEnumerable).IsAssignableFrom(newObj.GetType()))
            {
                if (typeof(IDictionary).IsAssignableFrom(newObj.GetType()))
                {
                    int i = 0;
                    foreach (DictionaryEntry entry in newObj as IDictionary)
                    {
                        string addedKeyPath = "Key[" + i.ToString() + "]";
                        string addedValuePath = "Value[" + i.ToString() + "]";

                        if (fullPath.Contains(path + addedKeyPath))
                        {
                            if (path + addedKeyPath == fullPath)
                            {
                                object key = DeserializeVar(data, path + addedKeyPath, entry.Key, ref offset, fullPath, out bool success);

                                if (success)
                                {
                                    if (dicKeys.Count == 0)
                                    { 
                                        actualUsingKeyName = path + addedKeyPath;
                                        actualUsingValueName = path + addedValuePath;
                                    }
                                    else
                                    {
                                        string[] pathParts = path.Split("\\");

                                        if (pathParts[0] != actualUsingKeyName && !path.Contains("\\"))
                                        {
                                            dicKeys.Clear();
                                            actualUsingKeyName = path + addedKeyPath;
                                            actualUsingValueName = path + addedValuePath;
                                        }
                                    }

                                    dicKeys.Enqueue(key);
                                }
                            }
                            else
                            {
                                OverWrite(data, (newObj as IDictionary)[entry.Key], (newObj as IDictionary)[entry.Key].GetType(), fullPath, ref offset, path + addedKeyPath + "\\", true);                                
                            }

                            return;
                        }
                        else if (fullPath.Contains(path + addedValuePath))
                        {
                            if (path + addedValuePath == fullPath)
                            {
                                object value = DeserializeVar(data, path + addedValuePath, entry.Value, ref offset, fullPath, out bool success);

                                if (success)
                                {
                                    string[] pathParts = path.Split("\\");

                                    if (actualUsingValueName == pathParts[0])
                                    { 
                                        (newObj as IDictionary)[dicKeys.Dequeue()] = value; 
                                    }
                                    else
                                    {
                                        dicKeys.Clear();
                                        (newObj as IDictionary)[entry.Key] = value;
                                    }
                                }
                            }
                            else
                            {
                                object key;

                                if (dicKeys.Count == 0)
                                {
                                    key = entry.Key;
                                    actualUsingKeyName = path + addedKeyPath;
                                    actualUsingValueName = path + addedValuePath;
                                }
                                else
                                {
                                    key = dicKeys.Dequeue();
                                }

                                dicKeys.Enqueue(key);

                                OverWrite(data, (newObj as IDictionary)[key], (newObj as IDictionary)[key].GetType(), fullPath, ref offset, path + addedValuePath + "\\", true);
                            }

                            return;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
                else if (typeof(ICollection).IsAssignableFrom(newObj.GetType()))
                {
                    int i = 0;
                    foreach (object element in newObj as ICollection)
                    {
                        string indexPath = "[" + i.ToString() + "]";
                    
                        if (path + indexPath == fullPath)
                        {
                            object var = DeserializeVar(data, path + indexPath, element, ref offset, fullPath, out bool success);

                            if (success)
                            {
                                //OverrideCollectionValue(ref newObj, i, var);
                                if (var is int)
                                {
                                    OverrideCollectionValue(ref newObj, i, (int)var);
                                }
                                else if (var is float)
                                {
                                    OverrideCollectionValue(ref newObj, i, (float)var);
                                }
                                else if (var is bool)
                                {
                                    OverrideCollectionValue(ref newObj, i, (bool)var);
                                }
                                else if (var is char)
                                {
                                    OverrideCollectionValue(ref newObj, i, (char)var);
                                }
                                else if (var is string)
                                {
                                    OverrideCollectionValue(ref newObj, i, (string)var);
                                }
                                else if (var is Vector2)
                                {
                                    OverrideCollectionValue(ref newObj, i, (Vector2)var);
                                }
                                else if (var is Vector3)
                                {
                                    OverrideCollectionValue(ref newObj, i, (Vector3)var);
                                }
                                else if (var is Quaternion)
                                {
                                    OverrideCollectionValue(ref newObj, i, (Quaternion)var);
                                }
                                else if (var is Color)
                                {
                                    OverrideCollectionValue(ref newObj, i, (Color)var);
                                }
                                else if (var is Transform)
                                {
                                    OverrideCollectionValue(ref newObj, i, (Transform)var);
                                }
                            }
                            break;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
            }
            else
            {
                //if (field.Name == pathParts[0])
                //{
                //    object value = DeserializeVar(data, pathParts[0], newObj, ref offset, fullPath, out bool success);
                //    if (success)
                //    {
                //        field.SetValue(obj, value);
                //        return value;
                //    }
                //    //object value = GetVarData(data, pathParts[0], newObj, ref offset);
                //    //field.SetValue(obj, value);
                //}
            }
        }

        private object DeserializeVar(byte[] data, string name, object obj, ref int offset, string fieldName, out bool success)
        {
            success = false;
            object value = null;

            if (obj is int)
            {
                value = NetDeserialization.DeserializeInt(name, data, ref offset, out success);
            }
            else if (obj is float)
            {
                value = NetDeserialization.DeserializeFloat(name, data, ref offset, out success);
            }
            else if (obj is bool)
            {
                value = NetDeserialization.DeserializeBool(name, data, ref offset, out success);
            }
            else if (obj is char)
            {
                value = NetDeserialization.DeserializeChar(name, data, ref offset, out success);
            }
            else if (obj is string)
            {
                value = NetDeserialization.DeserializeString(name, data, ref offset, out success);
            }
            else if (obj is Vector2)
            {
                value = NetDeserialization.DeserializeVector2(name, data, ref offset, out success);
            }
            else if (obj is Vector3)
            {
                value = NetDeserialization.DeserializeVector3(name, data, ref offset, out success);
            }
            else if (obj is Quaternion)
            {
                value = NetDeserialization.DeserializeQuaternion(name, data, ref offset, out success);
            }
            else if (obj is Color)
            {
                value = NetDeserialization.DeserializeColor(name, data, ref offset, out success);
            }
            else if (obj is Transform)
            {
                value = NetDeserialization.DeserializeTransform(name, data, ref offset, out success);
            }
            else if (obj is byte[])
            {
                value = NetDeserialization.DeserializeBytes(name, data, ref offset, out success);
            }
            else
            {
                value =  OverWrite(data, obj, obj.GetType(), name, ref offset, fieldName + "\\");
                success = true;
            }

            return success ? value : null;
        }

        private void OverrideCollectionValue<T>(ref object newObj, int i, T collectionValue)
        {
            ICollection collection = newObj as ICollection;

            Type type = newObj.GetType();
            Type typeGeneric = null;

            if (!type.IsArray)
            {
                typeGeneric = newObj.GetType().GetGenericTypeDefinition();
            }

            bool isAddedValue = collection.Count <= i;

            if (type.IsArray)
            {
                (newObj as T[])[i] = collectionValue;
            }
            else if (typeGeneric == typeof(List<>))
            {
                if (isAddedValue)
                {
                    (newObj as IList).Add(collectionValue);
                }
                else
                {
                    (newObj as IList)[i] = collectionValue;
                }
            }
            else if (typeGeneric == typeof(Queue<>))
            {
                Queue<T> originalQueue = newObj as Queue<T>;

                if (isAddedValue)
                {
                    originalQueue.Enqueue(collectionValue);
                }
                else
                {
                    Queue<T> tempQueue = new Queue<T>();

                    int count = originalQueue.Count;

                    for (int j = 0; j < count; j++)
                    {
                        if (j == i)
                        {
                            tempQueue.Enqueue(collectionValue);
                            originalQueue.Dequeue();
                        }
                        else
                        {
                            tempQueue.Enqueue(originalQueue.Dequeue());
                        }
                    }

                    for (int j = 0; j < count; j++)
                    {
                        originalQueue.Enqueue(tempQueue.Dequeue());
                    }
                }
            }
            else if (typeGeneric == typeof(Stack<>))
            {
                Stack<T> originalStack = newObj as Stack<T>;

                if (isAddedValue)
                {
                    originalStack.Push(collectionValue);
                }
                else
                {
                    Stack<T> tempStack = new Stack<T>();
                    int count = originalStack.Count;

                    for (int j = 0; j < count; j++)
                    {
                        if (j == i)
                        {
                            tempStack.Push(collectionValue);
                            originalStack.Pop();
                        }
                        else
                        {
                            tempStack.Push(originalStack.Pop());
                        }
                    }

                    for (int j = 0; j < count; j++)
                    {
                        originalStack.Push(tempStack.Pop());
                    }
                }
            }
        }

        //private object GetVarData(byte[] data, string name, object obj, ref int offset, string fullPath)
        //{
        //    bool success = false;
        //    object value = null;
        //
        //    if (obj is int)
        //    {
        //        value = NetDeserialization.DeserializeInt(name, data, ref offset, out success);
        //    }
        //    else if (obj is float)
        //    {
        //        value = NetDeserialization.DeserializeFloat(name, data, ref offset, out success);
        //    }
        //    else if (obj is bool)
        //    {
        //        value = NetDeserialization.DeserializeBool(name, data, ref offset, out success);
        //    }
        //    else if (obj is char)
        //    {
        //        value = NetDeserialization.DeserializeChar(name, data, ref offset, out success);
        //    }
        //    else if (obj is string)
        //    {
        //        value = NetDeserialization.DeserializeString(name, data, ref offset, out success);
        //    }
        //    else if (obj is Vector2)
        //    {
        //        value = NetDeserialization.DeserializeVector2(name, data, ref offset, out success);
        //    }
        //    else if (obj is Vector3)
        //    {
        //        value = NetDeserialization.DeserializeVector3(name, data, ref offset, out success);
        //    }
        //    else if (obj is Quaternion)
        //    {
        //        value = NetDeserialization.DeserializeQuaternion(name, data, ref offset, out success);
        //    }
        //    else if (obj is Color)
        //    {
        //        value = NetDeserialization.DeserializeColor(name, data, ref offset, out success);
        //    }
        //    else if (obj is byte[])
        //    {
        //        value = NetDeserialization.DeserializeBytes(name, data, ref offset, out success);
        //    }
        //    else
        //    {
        //        OverWrite(data, obj, obj.GetType(), name, ref offset, fullPath);
        //    }
        //
        //    return success ? value : null;
        //}

        #endregion

        #region READ_METHODS
        private List<List<byte>> Inspect(object obj, Type type, bool isFieldValue, string fieldName = "")
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
                        InspectValue(output, value, fieldName + field.Name);
                    }
                }
            }

            if (isFieldValue)
            {
                InspectValue(output, obj, fieldName);
            }

            if (typeof(ISync).IsAssignableFrom(type))
            {
                ISync a = (obj as ISync);
                byte[] bytes = a.Serialize();
                ConvertToMessage(output, bytes, fieldName);
            }

            if (type.BaseType != null)
            {
                foreach (List<byte> msg in Inspect(obj, type.BaseType, false, fieldName))
                {
                    output.Add(msg);
                }
            }

            return output;
        }

        private void InspectValue(List<List<byte>> output, object value, string fieldName)
        {
            if (typeof(IEnumerable).IsAssignableFrom(value.GetType()))
            {
                if (typeof(IDictionary).IsAssignableFrom(value.GetType()))
                {
                    int i = 0;

                    foreach (DictionaryEntry entry in value as IDictionary)
                    {
                        ConvertToMessage(output, entry.Key, fieldName + "Key[" + i.ToString() + "]");
                        ConvertToMessage(output, entry.Value, fieldName + "Value[" + i.ToString() + "]");

                        i++;
                    }
                }
                else if (typeof(ICollection).IsAssignableFrom(value.GetType()))
                {
                    int i = 0;

                    foreach (object element in value as ICollection)
                    {
                        ConvertToMessage(output, element, fieldName + "[" + i.ToString() + "]");
                        i++;
                    }
                }
            }
            else
            {
                ConvertToMessage(output, value, fieldName);
            }
        }

        private void ConvertToMessage(List<List<byte>> msgStack, object obj, string fieldName)
        {
            void SaveToSendIfChanged(object obj, List<byte> bytes)
            {
                if (SaveIfChanged(obj, fieldName))
                {
                    msgStack.Add(bytes);
                }
            }

            List<byte> bytes = NetSerialization.Serialize(obj, fieldName);
            
            if (bytes == null)
            {
                foreach (List<byte> msg in Inspect(obj, obj.GetType(), true, fieldName + "\\"))
                {
                    msgStack.Add(msg);
                }
            }
            else
            {
                SaveToSendIfChanged(obj, bytes);
            }
        }

        private void CallSyncMethods(object obj, byte[] dataBytes)
        {
            foreach (MethodInfo method in obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                SyncMethodAttribute attribute = method.GetCustomAttribute<SyncMethodAttribute>();

                if (attribute != null)
                {
                    method.Invoke(obj, new object[] { dataBytes });
                }
            }
        }

        //private List<byte> ConvertToMessage(object obj, string fieldName)
        //{
        //    List<byte> bytes = NetSerialization.Serialize(obj, fieldName);
        //
        //    if (bytes == null)
        //    {
        //        foreach (List<byte> msg in Inspect(obj, obj.GetType(), fieldName + "\\"))
        //        {
        //            bytes.AddRange(msg);
        //        }
        //    }
        //
        //    return bytes;
        //}
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

            return false;
        }
        #endregion
    }
}