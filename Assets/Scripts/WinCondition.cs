using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WinCondition", menuName = "ScriptableObjects/WinCondition", order = 1)]
public class WinCondition : ScriptableObject
{
    public enum WinConditionType
    {
        All,
        Some,
        None
    }
    public string roleName = "";

    public enum WinConditionEffect
    {
        Alive,
        Dead
    }
}
