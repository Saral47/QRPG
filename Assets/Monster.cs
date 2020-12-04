using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : Tile
{
    public override void Initialize()
    {
        tileType = "Monster";
        state.reward = -0.5f;
        actions = new List<Action>();
        bestAction = new Action("STAY", this, this);
    }
}
