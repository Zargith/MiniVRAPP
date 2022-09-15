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
}
