using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : Tile
{
    public override void Initialize()
    {
        tileType = "Exit";
        state.reward = 1;
        isTerminal = true;
        actions = new List<Action>();
        bestAction = new Action("EXIT", this, null);
    }

    public override void ResetTile()
    {
        state.reward = state.mReward;
    }
}
