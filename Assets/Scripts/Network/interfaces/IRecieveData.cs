using System.Net;

public interface IReceiveData
{
    void OnReceiveDataUdp(byte[] data, IPEndPoint ipEndpoint);
    void OnReceiveDataTcp(byte[] data, IPEndPoint ipEndpoint);
}