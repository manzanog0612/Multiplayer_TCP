using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ReflectionMessage
{
    private List<byte> data;

    public ReflectionMessage(List<byte> data)
    {
        this.data = data;
    }

    public List<byte> Data { get => data; }
}

public enum VALUE_TYPE
{
    INT,
    FLOAT,
    DOUBLE,
    CHAR,
    BOOL,
    VECTOR3
}

public class ReflectionHandler : MonoBehaviour
{
    private BindingFlags InstanceDeclaredOnlyFilter => BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

    public GameManager gameManager = null;
    public ClientHandler clientHandler = null;

    void Start()
    {

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

        ReflectionMessage reflectionMessage = new ReflectionMessage(dataBytes);

        CallSyncMethods(clientHandler.ClientNetworkManager, new List<object> () { reflectionMessage }.ToArray() );
    }

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
                                ConvertToMessage(output, entry.Key, fieldName + field.Name + "Key[" + i.ToString() + "]");
                                ConvertToMessage(output, entry.Value, fieldName + field.Name + "Value[" + i.ToString() + "]");
                                i++;
                            }
                        }
                        else if (typeof(ICollection).IsAssignableFrom(value.GetType()))
                        {
                            int i = 0;

                            foreach (object element in (value as ICollection))
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
        List<byte> SerializeMessage()
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

        List<byte> bytes = new List<byte>();
        bytes.AddRange(SerializeMessage());

        if (obj is int)
        {
            bytes.AddRange(IntSerialization.Serialize(obj));
            msgStack.Add(bytes);
        }
        else if (obj is float)
        {
            bytes.AddRange(FloatSerialization.Serialize(obj));
            msgStack.Add(bytes);
        }
        else if (obj is bool)
        {
            bytes.AddRange(BoolSerialization.Serialize(obj));
            msgStack.Add(bytes);
        }
        else if (obj is Vector3)
        {
            bytes.AddRange(Vector3Serialization.Serialize(obj));
            msgStack.Add(bytes);
        }
        else
        {
            foreach (List<byte> msg in Inspect(obj, obj.GetType(), fieldName + "\\"))
            {
                msgStack.Add(msg);
            }
        }
    }

    private void CallSyncMethods(object obj, object[] dataBytes)
    {
        foreach (MethodInfo method in obj.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
        {
            SyncMethodAttribute attribute = method.GetCustomAttribute<SyncMethodAttribute>();

            if (attribute != null)
            {
                Debug.Log(method.Name);
                method.Invoke(obj, dataBytes);
            }
        }
    }
}

