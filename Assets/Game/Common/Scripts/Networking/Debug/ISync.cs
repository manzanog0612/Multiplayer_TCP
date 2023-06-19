public interface ISync 
{
    public byte[] Serialize();
    public void Deserialize(byte[] msg);
}
