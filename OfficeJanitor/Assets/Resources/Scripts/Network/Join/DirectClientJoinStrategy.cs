using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

[CreateAssetMenu(fileName = "DirectClientJoin", menuName = "Strategy/Network/Client/DirectClientJoin")]
public class DirectClientJoinStrategy : BaseClientJoinStrategy
{
    private ushort _defaultPort = 7777;
    private const string _defaultIp = "127.0.0.1";

    public override void Join(string ipPortStr)
    {
        var networkMgr = NetworkManager.Singleton;
        var transport = networkMgr.GetComponent<UnityTransport>();

        var (ip, port) = ParseJoinCode(ipPortStr);
        transport.SetConnectionData(ip, port);

        // GameMetaEvents.Instance.OnHostConnected.Invoke(new NetworkJoinInfo()
        //     { NetworkJoinType = NetworkJoinType.Direct, JoinString = "Ask host for ip" });
        networkMgr.StartClient();
    }

    /// <summary>
    /// Parses the join code to extract the IP and port.
    /// Expected format: "IP:Port".
    /// </summary>
    /// <param name="ipPortStr"></param>
    /// <returns></returns>
    private (string IP, ushort Port) ParseJoinCode(string ipPortStr)
    {
        var parts = ipPortStr.Split(':');
        if (parts.Length < 1)
        {
            Debug.LogError("Join code is empty or invalid.");
            return (string.Empty, 0);
        }
        else if (parts.Length != 2)
        {
            Debug.LogWarning($"Join code has no port. Expected format: IP:Port. Defaulting to {_defaultPort}.");
            return (parts[0], _defaultPort);
        }

        string ip = parts[0];
        if (!ushort.TryParse(parts[1], out ushort port))
        {
            Debug.LogError("Invalid port in join code.");
            return (ip, 0);
        }

        return (ip, port);
    }
}