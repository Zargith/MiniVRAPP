using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InterfacePlayerManager : MonoBehaviour
{
    protected Role _role;

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
