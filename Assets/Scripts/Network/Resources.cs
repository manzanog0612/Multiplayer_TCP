using System.Net;

public struct DataReceived
{
    public byte[] data;
    public IPEndPoint ipEndPoint;
}

public struct ConnectionData
{
    public string server;
    public int port;

    public ConnectionData(string server, int port)
    {
        this.server = server;
        this.port = port;
    }
}
