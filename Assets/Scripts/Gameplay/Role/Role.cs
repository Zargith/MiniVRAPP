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
    public string roleGameplayScriptName = null;

    public Role(Role role) {
        this._name = role._name;
        this.namePlurial = role.namePlurial;
        this.description = role.description;
        this.quantity = role.quantity;
        this.winConditions = role.winConditions;
        this.roleGameplayScriptName = role.roleGameplayScriptName;
    }

    public Role(string name, string namePlurial, string description, int quantity, WinCondition[] winConditions, string roleGameplayScriptName) {
        this._name = name;
        this.namePlurial = namePlurial;
        this.description = description;
        this.quantity = quantity;
        this.winConditions = winConditions;
        this.roleGameplayScriptName = roleGameplayScriptName;
    }
}
