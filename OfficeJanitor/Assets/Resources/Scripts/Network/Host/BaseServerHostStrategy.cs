using UnityEngine;

public abstract class BaseServerHostStrategy : ScriptableObject, IServerHostStrategy
{
    public virtual async void Host()
    {
        throw new System.NotImplementedException();
    }
}