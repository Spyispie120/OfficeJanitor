using UnityEngine;

public abstract class BaseClientJoinStrategy : ScriptableObject, IClientJoinStrategy
{
    public virtual async void Join(string joinCode)
    {
        throw new System.NotImplementedException();
    }
}