using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    string role = "villagois(e)";
    [SerializeField] TextMeshProUGUI roleText;

    public void SetRole(string role)
    {
        this.role = role;
        roleText.text = role;
    }

    public string GetRole()
    {
        return role;
    }
}
