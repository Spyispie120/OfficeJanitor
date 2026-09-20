using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ConnectScreenUIHandler : MonoBehaviour
{
    [SerializeField] private BaseNetworkConnectionStrategy serverConnectionStrategy;
    [SerializeField] private Button _startHostButton;
    [SerializeField] private Button _startClientButton;
    [SerializeField] private TMP_InputField _inputField;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _startHostButton.onClick.AddListener(StartHost);
        _startClientButton.onClick.AddListener(StartClient);
    }

    private void StartHost()
    {
        serverConnectionStrategy.Host();
        _startHostButton.gameObject.SetActive(false);
        _startClientButton.gameObject.SetActive(false);
    }

    private void StartClient()
    {
        serverConnectionStrategy.Join(_inputField.text);
        _startHostButton.gameObject.SetActive(false);
        _startClientButton.gameObject.SetActive(false);
        _inputField.readOnly = true;
    }

    public void SetInputFieldText(string text)
    {
        _inputField.text = text;
        _inputField.readOnly = true;
    }

}
