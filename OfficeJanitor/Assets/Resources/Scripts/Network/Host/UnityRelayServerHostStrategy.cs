using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

[CreateAssetMenu(fileName = "UnityRelayServerHost", menuName = "Strategy/Network/Server/UnityRelayServerHost")]
public class UnityRelayServerHostStrategy : BaseServerHostStrategy
{
    public override async void Host()
    {
        try
        {
            var networkMgr = NetworkManager.Singleton;

            var relay = RelayService.Instance;
            Allocation allo = await relay.CreateAllocationAsync(3); // TODO: make game constant
            string joinCode = await relay.GetJoinCodeAsync(allo.AllocationId);
            Debug.Log($"JOIN CODE IS {joinCode}");
            GlobalEvent.Instance.OnHostConnected.Invoke(new NetworkJoinInfo()
            {
                NetworkJoinType = NetworkJoinType.Relay,
                JoinString = joinCode
            });

            var transport = networkMgr.GetComponent<UnityTransport>();
            RelayServerData relayServerData = AllocationUtils.ToRelayServerData(allo, "dtls");
            transport.SetRelayServerData(relayServerData);

            networkMgr.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }
}