public interface INetworkConnectionStrategy
{
    public void Init();
    public void Host();
    public void Join(string joinCode);
}