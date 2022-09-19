using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Role", menuName = "ScriptableObjects/Role", order = 1)]
public class Role : ScriptableObject
{
    public string _name = "";
    public string description = "";
    public int quantity = 0;
    public WinCondition[] winConditions = new WinCondition[0];

    public Role(Role role) {
        _name = role._name;
        description = role.description;
        quantity = role.quantity;
        winConditions = role.winConditions;
    }

    public Role(string name, string description, int quantity, WinCondition[] winConditions) {
        _name = name;
        this.description = description;
        this.quantity = quantity;
        this.winConditions = winConditions;
    }
}
