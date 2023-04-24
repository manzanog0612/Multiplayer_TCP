using System;
using System.Collections.Generic;
using System.Net;

using UnityEngine;

[Serializable]
public class Client
{
    public float timeStamp;
    public int id;
    public IPEndPoint ipEndPoint;
    public Dictionary<MESSAGE_TYPE, int> lastMessagesIds;
    public Vector3 position = Vector3.zero;
    public Color color = Color.white;

    public Client(IPEndPoint ipEndPoint, int id, float timeStamp, Dictionary<MESSAGE_TYPE, int> lastMessagesIds, Vector3 position, Color color)
    {
        this.timeStamp = timeStamp;
        this.id = id;
        this.ipEndPoint = ipEndPoint;
        this.lastMessagesIds = lastMessagesIds;
        this.position = position;
        this.color = color;
    }
}
