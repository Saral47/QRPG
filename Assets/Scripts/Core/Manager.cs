using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public struct StateAction
{
    public StateAction(Tile s, Action a)
    {
        state = s;
        action = a;
    }
    public Tile state;
    public Action action;
}
public class Manager : Singleton<Manager>
{

    Transform tiles;

    public int worldSizeX;
    public int worldSizeY;
    public int tileSize;
    public int numTilesX;
    public int numTilesY;
    private int tileCounter;


    public Tile START = null;
    public List<Tile> END = new List<Tile>();

    public int maxEpoch;
    public bool pauseQlearning;
    public bool stepThroughQlearning;
    public bool pathFound { get; set; }
    public float discountFactor;
    public float alpha;
    public float epsilon;
    public float decayRate;

    private int previousState;
    public int highlightState;
    public bool deleteState = false;
    public float delay;

    List<Tile> path = new List<Tile>();
    public GameObject land,monster,gold,exit,player;
    public TextMeshProUGUI Text;
    public TextMeshProUGUI QText;
    public TextMeshProUGUI PathText;
    public GameObject activeGameObject;
    public Coroutine QLearnCoroutine = null;
    public Dictionary<(int x, int y), GameObject> tileMap = new Dictionary<(int x, int y), GameObject>();   
    //QvalueTable[State(x,y),Action] = float q-value 
    public Dictionary<((int x, int y), string), float> QVALUETABLE = new Dictionary<((int x, int y), string), float>();

    private void Awake()
    {

       

        if(decayRate == 0)
            decayRate = epsilon / maxEpoch*1.33f;

        pathFound = false;


        tiles = GameObject.FindGameObjectWithTag("Tiles").transform;
        highlightState = 1;
        previousState = 1;
        activeGameObject = land;
        tileCounter = 0;
    }
   

    public void AddTile(int x, int y)
    {
       
        if (tileMap.ContainsKey((x, y)))
        {
            Debug.LogError("State Already Present : ("+x+", "+y);
            return;
        }
        GameObject go = Instantiate(activeGameObject, new Vector3(x, 0.5f+tileSize/2, y), Quaternion.identity);
        go.transform.localScale = new Vector3(tileSize, tileSize, tileSize);
        Tile selfTile = go.GetComponent<Tile>();


        if (START != null && selfTile.isStart == true)
        {
            Debug.LogError("PLAYER ALREADY PRESENT");
            Destroy(go);
            return;
        }

        go.transform.SetParent(tiles);
        go.GetComponent<Tile>().tileType = activeGameObject.GetComponent<Tile>().tileType;

        if (selfTile.isStart)
        {
            START = selfTile;

        }

        if (selfTile.isTerminal)
        {
            END.Add(selfTile);
        }


        selfTile.tileID = tileCounter++;
        selfTile.SetState(x, y); selfTile.Initialize();

        Dictionary<Tile, string> neigbhorTileAction = GetSurroundingTiles(selfTile.state);

      
        foreach (KeyValuePair<Tile, string> p in neigbhorTileAction)
        {
            Tile neighbor = p.Key;


            Action myAction = new Action(p.Value, selfTile, neighbor);
            Action neighborAction = new Action(myAction.InvertDirection(myAction.actionName), neighbor, selfTile);

            if (selfTile.isTerminal == false)
            {
                selfTile.AddAction(myAction);
                QVALUETABLE.Add(((selfTile.state.x, selfTile.state.y), myAction.actionName), 0f);
            }
            if (neighbor.isTerminal == false)
            {
                neighbor.AddAction(neighborAction);
                QVALUETABLE.Add(((neighbor.state.x, neighbor.state.y), neighborAction.actionName), 0f);
            }            
           
        }
        
        go.GetComponent<Tile>().actions = selfTile.actions;
        tileMap.Add((x, y), go);
              
    }

    private void Update()
    {
        Text.text = "Total Nodes: " + tileMap.Count.ToString() + Environment.NewLine;
        QText.text = "QTABLE SIZE: " + QVALUETABLE.Count.ToString() + Environment.NewLine;
        PathText.text = "PATH" + Environment.NewLine;

        if (!deleteState)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                highlightState = 1;
                activeGameObject = land;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                highlightState = 2;
                activeGameObject = player;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                highlightState = 3;
                activeGameObject = gold;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                highlightState = 4;
                activeGameObject = exit;
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                highlightState = 5;
                activeGameObject = monster;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (START == null || END.Count == 0)
                {
                    Debug.LogError("MISSING PLAYER OR GOAL");
                }
                else
                {
                    if (QLearnCoroutine == null)
                    {
                        QLearnCoroutine = StartCoroutine(QLearning.instance.QLearn());
                    }

                }
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                if (pathFound)
                {
                    PathText.text = "";
                    path = QLearning.instance.GetPath(START);

                    foreach (Tile tile in path)
                    {
                        GameObject t = tile.gameObject;
                        if (t.transform.position.y < 2)
                            t.transform.Translate(new Vector3(0f, 0.75f, 0f));
                    }
                }
            }
        }

        //if (Input.GetKeyDown(KeyCode.D))
        //{
        //    deleteState = !deleteState;
        //    if (deleteState)
        //    {
        //        previousState = highlightState;
        //        highlightState = 6;
        //    }
        //    else
        //    {
        //        highlightState = previousState;
        //    }
            
        //}


        Debug.Log("TILEMAP: " + tileMap.Count);
        foreach (KeyValuePair<(int,int),GameObject> p in tileMap)
        {
            Text.text = Text.text + "(" + p.Key.Item1 + " , " + p.Key.Item2 + ") Actions: "+p.Value.GetComponent<Tile>().actions.Count +Environment.NewLine;
        }

        foreach(KeyValuePair< ((int x, int y), string), float> pair in QVALUETABLE)
        {
            QText.text = QText.text + "(" + pair.Key.Item1.x + " , " + pair.Key.Item1.y + "), " + pair.Key.Item2 + " QValue: " + pair.Value+Environment.NewLine; ;
        }


        foreach (Tile t in path)
        {
            PathText.text = PathText.text + "---> (" + t.state.x + " , " + t.state.y + ")";
        }
    }

    public void ResetTiles()
    {
        foreach (Tile end in END)
            end.ResetTile();
    }

    public Action GetBestAction(Tile currentTile, out float bestValue)
    {

        Action bestAction = null;
        List<Action> availableActions = currentTile.actions;

        if (currentTile.actions.Count == 0)
        {
           
            bestValue = 0f;
            return null;
        }


        bestValue = -9999;
        
        foreach(Action action in availableActions)
        {
            if(QVALUETABLE[((currentTile.state.x, currentTile.state.y), action.actionName)] > bestValue)
            {
                bestAction = action;
                bestValue = QVALUETABLE[((currentTile.state.x, currentTile.state.y), action.actionName)];
            }
        }
        //Debug.Log("BEST ACTION HERE IS: "+bestAction.actionName);
        return bestAction;
    }

    public void TakeAction(Action action, out Tile nextTile, out float reward, out bool isTerminal)
    {
        nextTile = action.nextTile;
        reward = ComputeReward(action);
        isTerminal = nextTile.isTerminal;
    }

    public float ComputeReward(Action action)
    {

        


        return action.nextTile.state.reward;
    }

    public Dictionary<Tile, string> GetSurroundingTiles(State currentState)
    {
        Dictionary<Tile,string> neighborTiles = new Dictionary<Tile, string>();
        State neighborState;


        neighborState = new State(currentState.x + tileSize, currentState.y + tileSize);
        if (tileMap.ContainsKey((neighborState.x, neighborState.y))) // NE
        {
            //  Debug.Log("NorthEast");
            neighborTiles.Add(tileMap[(neighborState.x, neighborState.y)].GetComponent<Tile>(), "NorthEast");
        }

        neighborState = new State(currentState.x + tileSize, currentState.y - tileSize);
        if (tileMap.ContainsKey((neighborState.x, neighborState.y))) // SE
        {
            //  Debug.Log("SouthEast");
            neighborTiles.Add(tileMap[(neighborState.x, neighborState.y)].GetComponent<Tile>(), "SouthEast");
        }

        neighborState = new State(currentState.x - tileSize, currentState.y + tileSize);
        if (tileMap.ContainsKey((neighborState.x, neighborState.y))) // NW
        {
            //  Debug.Log("NorthWest");
            neighborTiles.Add(tileMap[(neighborState.x, neighborState.y)].GetComponent<Tile>(), "NorthWest");
        }

        neighborState = new State(currentState.x - tileSize, currentState.y - tileSize);
        if (tileMap.ContainsKey((neighborState.x, neighborState.y))) // SW
        {
            // Debug.Log("SouthWest");
            neighborTiles.Add(tileMap[(neighborState.x, neighborState.y)].GetComponent<Tile>(), "SouthWest");
        }


        neighborState = new State (currentState.x + tileSize, currentState.y );
        if (tileMap.ContainsKey((neighborState.x, neighborState.y))) // E
        {
           // Debug.Log("East");
            neighborTiles.Add(tileMap[(neighborState.x,neighborState.y)].GetComponent<Tile>(),"East");
        }


        neighborState = new State(currentState.x - tileSize, currentState.y);
        if (tileMap.ContainsKey((neighborState.x, neighborState.y)))// W
        {
           // Debug.Log("West");
            neighborTiles.Add(tileMap[(neighborState.x, neighborState.y)].GetComponent<Tile>(), "West");
        }

        neighborState = new State(currentState.x, currentState.y +  tileSize);
        if (tileMap.ContainsKey((neighborState.x, neighborState.y))) // N
        {
           // Debug.Log("North");
            neighborTiles.Add(tileMap[(neighborState.x, neighborState.y)].GetComponent<Tile>(), "North");
        }

        neighborState = new State(currentState.x, currentState.y - tileSize);
        if (tileMap.ContainsKey((neighborState.x, neighborState.y))) // S
        {
           // Debug.Log("South");
            neighborTiles.Add(tileMap[(neighborState.x, neighborState.y)].GetComponent<Tile>(), "South");
        }

        
        return neighborTiles;

    }



}
