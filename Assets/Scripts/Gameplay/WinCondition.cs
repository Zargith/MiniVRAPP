using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WinCondition", menuName = "ScriptableObjects/WinCondition", order = 1)]
public class WinCondition : ScriptableObject
{

    public bool not = false;
    public string roleName = "";
    public WinConditionType winConditionType = WinConditionType.All;
    public int quantity = 0;
    public WinConditionStatus winConditionStatus = WinConditionStatus.Dead;
}
