using UnityEngine;

public class UIHandler : MonoBehaviour
{
    public static UIHandler Instance { get; private set; }
    [SerializeField] private GameObject _connectScreenUIHanderPrefab;

    public ConnectScreenUIHandler ConnectScreenUIHandler { get; private set; }

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

    public void ShowConnectScreenUI()
    {
        if (ConnectScreenUIHandler == null)
        {
            GameObject connectScreenUIHandlerObj = Instantiate(_connectScreenUIHanderPrefab);
            ConnectScreenUIHandler = connectScreenUIHandlerObj.GetComponent<ConnectScreenUIHandler>();
        }
        ConnectScreenUIHandler.gameObject.SetActive(true);
    }
}