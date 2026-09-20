using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public abstract class BaseNetworkConnectionStrategy : ScriptableObject, INetworkConnectionStrategy
{
    [SerializeField]
    private BaseServerHostStrategy _serverHostStrategy;
    [SerializeField]
    private BaseClientJoinStrategy _clientJoinStrategy;

    public virtual async void Init()
    {
        await UnityServices.InitializeAsync();

        var authService = AuthenticationService.Instance;
        authService.SignedIn += () =>
        {
            Debug.Log($"Signed in {authService.PlayerId}");
        };
        await authService.SignInAnonymouslyAsync();
    }

    public virtual async void Host()
    {
        _serverHostStrategy?.Host();
    }

    public virtual async void Join(string joinCode)
    {
        _clientJoinStrategy?.Join(joinCode);
    }
}