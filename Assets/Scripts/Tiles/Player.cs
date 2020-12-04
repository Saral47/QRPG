using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Tile
{
    public override void Initialize()
    {
        tileType = "Player";
        state.reward = -0.01f;
        actions = new List<Action>();
        bestAction = new Action("STAY", this, this);
    }

}
