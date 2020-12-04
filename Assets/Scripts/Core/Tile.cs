using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;


[System.Serializable]
public struct State
{
    public State(int xin, int yin, float r = 0)
    {
        x = xin;
        y = yin;
        reward = r;
        mReward = reward;
    }
    
    public int x { get; set; }
    public int y { get; set; }
    //public float reward { get; set; }

    //public float x;
    //public float y;
    public float reward;
    public float mReward { get; set; }
}

public class Tile : MonoBehaviour
{
    public string tileType; 
    public State state;
    public float tileID { get; set; }
    public List<Action> actions;

    public Action bestAction;
    public Dictionary<string, int> actionCounterList = new Dictionary<string, int>();
    public bool isTerminal;
    public bool isStart;

    public TextMeshPro displayText;
    private void Start()
    {
        displayText = transform.GetComponent<TextMeshPro>();      
       
    }
    public void SetState(int x, int y, float r=0)
    {
        state.x = x;
        state.y = y;
        state.reward = r;
       
    }
    virtual public void ResetTile()
    {

    }
    virtual public void Initialize()
    {
        tileType = "Tile";
        actions = new List<Action>();
        bestAction = new Action("STAY", this, this);
    }
    public void SetStatePosition(int x, int y)
    {
        state.x = x;
        state.y = y;
    }
    public void SetStateReward(float r)
    {
        state.reward = r;
    }

    public void AddAction(Action action)
    {
        if (actions.Any() == false)
             actions = new List<Action>();    
        
        actions.Add(action);
        actionCounterList.Add(action.actionName, 0);
        Debug.Log("ADDED ACTION " + action.actionName + " FROM (" + action.currentTile.state.x+", "+ action.currentTile.state.y + ") TO ("
            + action.nextTile.state.x + ", " + action.nextTile.state.y+")");


       // Debug.Log("AFTER ADDING " + actions.Count);
    }

    public Action GetAction(string actionName)
    {
        foreach(Action action in actions)
        {
            if (action.actionName == actionName)
                return action;
        }

        Debug.LogError("ACTION DOES NOT EXIST IN LIST");
        return null;
    }

    virtual public void TileUpdate()
    {

    }

    public void DeleteSelf()
    {
        DeleteNeighborEdges();
        Manager.instance.tileMap.Remove((state.x, state.y));
        Destroy(this.gameObject);
    }
    private void DeleteNeighborEdges()
    {
        Dictionary<Tile, string> neighbors = new Dictionary<Tile, string>();

        neighbors = Manager.instance.GetSurroundingTiles(this.state);
    
        foreach(KeyValuePair<Tile,string> pair in neighbors)
        {
            string action = HasActionToTile(this);
            if(action != "")
            {
                DeleteAction(action);
            }
        }

       
    }
    
    void DeleteAction(string actionName)
    {
        if (bestAction.actionName == actionName)
            bestAction = null;

        foreach(Action action in actions)
        {
            if (action.actionName == actionName)
            {
                Debug.Log("REMOVING");
                actions.Remove(action);
                
                Manager.instance.QVALUETABLE.Remove(((action.currentTile.state.x, action.currentTile.state.y), actionName));
            }

        }
    }
    public string HasActionToTile(Tile targetTile)
    {

        foreach(Action action in actions)
        {
            if (action.nextTile.state.x == targetTile.state.x && action.nextTile.state.x == targetTile.state.x)
                return action.actionName;
        }

        return "";
    }

    public void Update()
    {
        transform.GetChild(0).GetComponent<TextMeshPro>().text = "(" + state.x + ", " + state.y + ")" + System.Environment.NewLine;
        transform.GetChild(0).GetComponent<TextMeshPro>().text += bestAction.actionName;        
    }
}
