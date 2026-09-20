using UnityEngine;

public class GlobalGameState : MonoBehaviour
{
    public static GlobalGameState Instance { get; private set; }
    [SerializeField] private BaseNetworkConnectionStrategy _networkConnectionStrategy;

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
    private void Start()
    {
        _networkConnectionStrategy.Init();
        UIHandler.Instance.ShowConnectScreenUI();
        BindEvents();
    }

    private void BindEvents()
    {
        GlobalEvent.Instance.OnHostConnected.AddListener(OnHostConnected);
    }

    private void OnHostConnected(NetworkJoinInfo joinInfo)
    {
        // Handle the event when the host is connected
        UIHandler.Instance.ConnectScreenUIHandler.SetInputFieldText(joinInfo.JoinString);

    }
}