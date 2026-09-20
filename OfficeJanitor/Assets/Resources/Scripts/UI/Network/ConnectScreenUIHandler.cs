using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectScreenUIHandler : MonoBehaviour
{
    [SerializeField] private Button StartHostButton;
    [SerializeField] private Button StartClientButton;

    private NetworkManager networkManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        networkManager = NetworkManager.Singleton;
        StartHostButton.onClick.AddListener(StartHost);
        StartClientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        networkManager.StartHost();
    }

    private void StartClient()
    {
        networkManager.StartClient();
    }
}
