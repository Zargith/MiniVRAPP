using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    string role = "villagois(e)";
    [SerializeField] TextMeshProUGUI roleText;
    Role _role;

    public void SetRole(Role role)
    {
        this._role = role;
        roleText.text = "Ton rôle : " + role._name;
    }

    public string GetRole()
    {
        return role;
    }
}
