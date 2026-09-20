using UnityEngine;
using UnityEngine.Events;

public class GlobalEvent : MonoBehaviour
{
    public static GlobalEvent Instance { get; private set; }
    public UnityEvent<NetworkJoinInfo> OnHostConnected { get; private set; } = new UnityEvent<NetworkJoinInfo>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
}

public class NetworkJoinInfo
{
    public NetworkJoinType NetworkJoinType { get; set; }
    public string JoinString { get; set; }
}

public enum NetworkJoinType
{
    Relay,
    Direct
}