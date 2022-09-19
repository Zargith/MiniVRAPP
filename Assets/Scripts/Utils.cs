using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum WinConditionType
{
    All,
    Some,
    None
}

public enum WinConditionStatus
{
    Alive,
    Dead
}

public enum GameCycle
{
    SetDay,
    DeadsAnnouncement,
    VillagersVote,
    // EndVillageVote,
    SetNight,
    WerewolvesVote,
    // EndWerewolvesVote
}