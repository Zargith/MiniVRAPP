using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WitchGameplay : RoleGameplay
{
    void Start() {}

    bool _potionSavedLife = false;
    bool _potionKilled = false;

    public bool potionSavedLife() {
        return _potionSavedLife;
    }

    public bool potionKilled() {
        return _potionKilled;
    }

    public void useSaveLifePotion() {
        _potionSavedLife = true;
    }

    public void useKillPotion() {
        _potionKilled = true;
    }
}
