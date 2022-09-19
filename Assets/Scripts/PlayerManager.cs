using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI roleText;
    Role _role;

    public void SetRole(Role role)
    {
        this._role = role;
        roleText.text = "Ton rôle : " + role._name;
    }

    public Role GetRole()
    {
        return _role;
    }

    public void Die()
    {
        
    }

}
