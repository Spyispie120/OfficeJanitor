using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "DirectServerHost", menuName = "Strategy/Network/Server/DirectServerHost")]
public class DirectServerHostStrategy : BaseServerHostStrategy
{
    public override void Host()
    {
        // GameMetaEvents.Instance.OnHostConnected.Invoke(new NetworkJoinInfo()
        //     { NetworkJoinType = NetworkJoinType.Direct, JoinString = "1-800-ip-doesnt-exist" });
        NetworkManager.Singleton.StartHost();
    }
}