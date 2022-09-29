using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Role", menuName = "ScriptableObjects/Role", order = 1)]
public class Role : ScriptableObject
{
    public string _name = "";
    public string namePlurial = "";
    public string description = "";
    public int quantity = 0;
    public WinCondition[] winConditions = new WinCondition[0];

    public Role(Role role) {
        _name = role._name;
        namePlurial = role.namePlurial;
        description = role.description;
        quantity = role.quantity;
        winConditions = role.winConditions;
    }

    public Role(string name, string namePlurial, string description, int quantity, WinCondition[] winConditions) {
        this._name = name;
        this.namePlurial = namePlurial;
        this.description = description;
        this.quantity = quantity;
        this.winConditions = winConditions;
    }
}
