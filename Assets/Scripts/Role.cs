using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Role", menuName = "ScriptableObjects/Role", order = 1)]
public class Role : ScriptableObject
{
    public string _name = "";
    public string description = "";
    public int quantity = 0;
    public string[] passiveAbilities = new string[0];
    public string[] activeAbilities = new string[0];
    public string[] nightAbilities = new string[0];
    public string[] dayAbilities = new string[0];
    public WinCondition[] winConditions = new WinCondition[0];

    public Role(Role role) {
        _name = role._name;
        description = role.description;
        quantity = role.quantity;
        passiveAbilities = role.passiveAbilities;
        activeAbilities = role.activeAbilities;
        nightAbilities = role.nightAbilities;
        dayAbilities = role.dayAbilities;
        winConditions = role.winConditions;
    }

    public Role(string name, string description, int quantity, string[] passiveAbilities, string[] activeAbilities, string[] nightAbilities, string[] dayAbilities, WinCondition[] winConditions) {
        _name = name;
        this.description = description;
        this.quantity = quantity;
        this.passiveAbilities = passiveAbilities;
        this.activeAbilities = activeAbilities;
        this.nightAbilities = nightAbilities;
        this.dayAbilities = dayAbilities;
        this.winConditions = winConditions;
    }
}
