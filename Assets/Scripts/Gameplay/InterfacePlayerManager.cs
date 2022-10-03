using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InterfacePlayerManager : MonoBehaviour
{
    protected Role _role;
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected AudioClip deathSound;

    void Awake() {
        audioSource.loop = false;
        audioSource.clip = deathSound;
    }

    public virtual void SetRole(Role role)
    {
        this._role = role;
    }

    public Role GetRole()
    {
        return _role;
    }

    public abstract void Die(string reason = "");
}
