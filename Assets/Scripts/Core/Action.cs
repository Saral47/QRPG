using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Action
{
    public Action(string name)
    {
        actionName = name;
    }
    public Action(string name, Tile cTile, Tile nTile)
    {
        actionName = name;
        currentTile = cTile;
        nextTile = nTile;

        if (name == "NorthEast" || name == "NorthWest" || name == "SouthWest" || name == "SouthEast")
            transitionRewardFactor = 1f;
        else
            transitionRewardFactor = 1f;
    }

    public string actionName;
    public Tile nextTile { get; set; }
    public Tile currentTile { get; set; }
    public float transitionRewardFactor { get; set; }
    
    public string InvertDirection(string dir)
    {
        if (dir == "East")
            return "West";

        if (dir == "West")
            return "East";

        if (dir == "North")
            return "South";

        if (dir == "South")
            return "North";

        if (dir == "NorthEast")
            return "SouthWest";

        if (dir == "SouthWest")
            return "NorthEast";

        if (dir == "NorthWest")
            return "SouthEast";

        if (dir == "SouthEast")
            return "NorthWest";

        return null;

    }
}
