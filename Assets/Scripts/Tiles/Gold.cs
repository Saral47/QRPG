using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Gold : Tile
{
    bool activatedOnce = false;
    public override void TileUpdate()
    {

        //if (activatedOnce == false)
        //{
        //    //foreach (Tile end in Manager.instance.END)
        //    //{
        //    //    Debug.Log("INCREASING MORDOR VALUE");
        //    //    end.state.reward += 1000;
        //    //}
        //    state.reward = -0.2f;
        //}
        //activatedOnce = true;
    }
    public override void Initialize()
    {
        tileType = "Gold";
        activatedOnce = false;
        state.reward = 0.05f;
        actions = new List<Action>();
        bestAction = new Action("STAY", this, this);
       // isTerminal = true;
    }

    public override void ResetTile()
    {
        activatedOnce = false;
        state.reward = state.mReward;
    }
}
