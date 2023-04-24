using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ServerData
{
    public int id = 0;
    public int port = 0;
    public int amountPlayers = 0;

    public ServerData(int id, int port, int amountPlayers)
    {
        this.id = id;
        this.port = port;
        this.amountPlayers = amountPlayers;
    }
}
