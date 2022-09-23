using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerManager : InterfacePlayerManager
{
    [SerializeField] TextMeshProUGUI roleText;
    [SerializeField] GameObject youDiedPanel;
    [SerializeField] TextMeshProUGUI youDiedText;

    public override void SetRole(Role role)
    {
        base.SetRole(role);
        roleText.text = "Ton rôle : " + role._name;
    }

    public override void Die(string reason = "")
    {
        youDiedPanel.SetActive(true);
        youDiedText.text = reason;
    }

}
