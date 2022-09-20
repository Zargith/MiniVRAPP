using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectPlayer : MonoBehaviour
{
    int nbOfSelection = 0;

    public void select()
    {
        nbOfSelection++;
        GameCycle gameCycle = GameManager.Instance.getGameCycleStep();
        if (gameCycle == GameCycle.VillagersVote)
            GameManager.Instance.finishVillagersVote();
        else if (gameCycle == GameCycle.WerewolvesVote)
            GameManager.Instance.finishWerewolvesVote();
    }

    public int get()
    {
        return nbOfSelection;
    }

    public void reset()
    {
        nbOfSelection = 0;
    }
}
