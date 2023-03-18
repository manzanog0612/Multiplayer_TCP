using System.Net;

public struct DataReceived
{
    public byte[] data;
    public IPEndPoint ipEndPoint;

    public DataReceived(byte[] data, IPEndPoint ipEndPoint)
    {
        this.data = data;
        this.ipEndPoint = ipEndPoint;
    }
}

public struct ConnectionData
{
    public IPAddress server;
    public int port;

    public ConnectionData(IPAddress server, int port)
    {
        this.server = server;
        this.port = port;
    }
}
