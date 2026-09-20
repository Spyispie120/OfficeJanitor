using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

[CreateAssetMenu(fileName = "UnityRelayClientJoin", menuName = "Strategy/Network/Client/UnityRelayClientJoin")]
public class UnityRelayClientJoinStrategy : BaseClientJoinStrategy
{
    public override async void Join(string joinCode)
    {
        try
        {
            var networkMgr = NetworkManager.Singleton;

            var relay = RelayService.Instance;
            JoinAllocation allo = await relay.JoinAllocationAsync(joinCode);

            var transport = networkMgr.GetComponent<UnityTransport>();
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allo, "dtls");
            transport.SetRelayServerData(relayServerData);

            networkMgr.StartClient();
            // GameMetaEvents.Instance.OnHostConnected.Invoke(new NetworkJoinInfo() 
            //     { NetworkJoinType = NetworkJoinType.Relay, JoinString = joinCode });
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }
}