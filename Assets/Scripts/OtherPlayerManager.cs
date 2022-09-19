using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayerManager : MonoBehaviour
{
    Role _role;

    public void SetRole(Role role)
    {
        this._role = role;
    }

    public Role GetRole()
    {
        return _role;
    }

    public void Die()
    {
        
    }

}
