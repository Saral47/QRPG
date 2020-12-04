using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.IO;
using System;
public class QLearning : Singleton<QLearning>
{
    public float epsilon;
    public float decayRate;
    public int maxEpochs;
    public float discountFactor;
    public float alpha;

    

    Tile currentTile;
    Tile startTile;
    Tile nextTile = null;

    float currentTileQValue = 0;
    float nextTileQValue = 0;

    float reward = 0;
    float sample = 0;
    bool isTerminal = false;

    int epoch = 0;
    void Start()
    {
        epsilon = Manager.instance.epsilon;
        decayRate = Manager.instance.decayRate;
        maxEpochs = Manager.instance.maxEpoch;
        discountFactor = Manager.instance.discountFactor;
        alpha = Manager.instance.alpha; // Learning rate
    }

    public void Initialize()
    {
        epoch = 0;
        
        epsilon = Manager.instance.epsilon;
        decayRate = Manager.instance.decayRate;
        maxEpochs = Manager.instance.maxEpoch;
        discountFactor = Manager.instance.discountFactor;
        alpha = Manager.instance.alpha; // Learning rate

            
        startTile = Manager.instance.START;
        currentTile = startTile;
        nextTile = null;

        currentTileQValue = 0;
        nextTileQValue = 0;

        reward = 0;
        sample = 0;
        isTerminal = false;

    }
    public IEnumerator QLearn()
    {
        
        Initialize();
        
        File.Delete(Application.dataPath + "/saved_data.csv");

        Debug.Log("QLEARNING PARAMETERS: "+ "MI: "+maxEpochs+ " GAMMA: "+discountFactor+" ALPHA: "+alpha+" EPS: " + epsilon+" EPS_DECAY:"+decayRate+ "(" + startTile.state.x + " " + startTile.state.y + ")");
        while (epoch <= maxEpochs)
        {

            epoch++;
            Debug.Log("STARTING AT (" + currentTile.state.x + " " + currentTile.state.y + ")");
            currentTile = startTile;
            int numSteps = 0;
            float cummulativeReward = 0;
            while (true)
            {
                
                if (numSteps > 1000)
                {
                    Debug.LogError("EPOCH: "+epoch+ "COULDNT REACH THE END IN GIVEN STEPS");
                    break;
                }
                numSteps++;

                currentTile.TileUpdate();

               
               Action action = EpsilonGreedyMod(currentTile, epsilon);
              // Action action = UCB_1(currentTile);
               //Action action = BoltzMann(currentTile);

                if (action == null && currentTile.isTerminal == false)
                {
                    Debug.LogError("NO AVAILABLE ACTIONS, RESETTING");
                    currentTile = startTile;
                    Manager.instance.ResetTiles();
                    break;
                }

                currentTile.bestAction = action;
                currentTile.actionCounterList[action.actionName] += 1;
                Manager.instance.TakeAction(action, out nextTile, out reward, out isTerminal);

                
                cummulativeReward = cummulativeReward + reward;

                if (isTerminal)
                {
                    Debug.Log("--------LEVEL COMPLETE--------" );
                    Manager.instance.pathFound = true;
                    sample = reward*action.transitionRewardFactor;
                    float qval2 = (1 - alpha) * currentTileQValue + alpha * sample;
                    Manager.instance.QVALUETABLE[((currentTile.state.x, currentTile.state.y), action.actionName)] = qval2;
                    break;
                }
                else
                {
                    Action nextAction = Manager.instance.GetBestAction(nextTile, out nextTileQValue);
                    sample = (reward + discountFactor * nextTileQValue)*action.transitionRewardFactor;
                }                
               
                float qval = (1 - alpha) * currentTileQValue + alpha * sample;
                Manager.instance.QVALUETABLE[((currentTile.state.x, currentTile.state.y), action.actionName)] = qval;


                if (isTerminal)
                    currentTile = startTile;
                else
                    currentTile = nextTile;               
            }


            // For epsilon decay
            if (epsilon > 0.1)
                epsilon = epsilon - decayRate;

            Debug.Log("#############  EPOCH: " + epoch + " STEPS TAKEN: " + numSteps + " Cumulative Reward " + cummulativeReward);
            string[] output = new string[3];
            output[0] = epoch.ToString(); output[1] = numSteps.ToString(); output[2] = cummulativeReward.ToString();

            Savecsv(output);

            if (Manager.instance.pauseQlearning == true && Manager.instance.stepThroughQlearning == false)
            {
                yield return new WaitForSeconds(Manager.instance.delay);
            }
            else if (Manager.instance.pauseQlearning == false && Manager.instance.stepThroughQlearning == true)
            {
                yield return new WaitForSeconds(0.1f);
                yield return StartCoroutine(WaitForKeyDown(KeyCode.Space));
            }

            
        }

        StopAllCoroutines();
        Manager.instance.QLearnCoroutine = null;


    }
   // public Action B

    public Action BoltzMann(Tile currentTile)
    {
        Action bestAction = null;

        float T = 1.5f;
        float totalBoltzMann = 0;
        float bestValue = -1*Mathf.Infinity;
        foreach (Action action in currentTile.actions)
            totalBoltzMann = totalBoltzMann + Mathf.Pow((float)Math.E, Manager.instance.QVALUETABLE[((currentTile.state.x, currentTile.state.y), action.actionName)]/T);
        
        foreach(Action action in currentTile.actions)
        {
            float boltMannProb = Mathf.Pow((float)Math.E, Manager.instance.QVALUETABLE[((currentTile.state.x, currentTile.state.y), action.actionName)] / T) / totalBoltzMann;

            if(boltMannProb > bestValue)
            {
                bestValue = boltMannProb;
                bestAction = action;
            }
        }

        return bestAction;
    }
    public Action UCB_1(Tile currentTile)
    {
        float C = 0.025f;
        int TotalActionsTaken = 0;
        
        Dictionary<string,float> bonus = new Dictionary<string, float>();
        foreach(KeyValuePair<string, int> pair in currentTile.actionCounterList)
        {
            TotalActionsTaken += pair.Value;
        }

        foreach (KeyValuePair<string, int> pair in currentTile.actionCounterList)
        {
            float ucbVal = 100 * C * Mathf.Sqrt(2 * Mathf.Log(TotalActionsTaken) / currentTile.actionCounterList[pair.Key]);
            bonus.Add(pair.Key, ucbVal);
            bonus[pair.Key] += Manager.instance.QVALUETABLE[((currentTile.state.x, currentTile.state.y), pair.Key)];
        }

        string bestAction = bonus.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

        return currentTile.GetAction(bestAction);
    }

    public Action EpsilonGreedyMod(Tile currentTile, float epsilon)
    {
       // Debug.Log("***********************************************************************************");
        List<Action> actions = currentTile.actions;
        Dictionary<string,float> actionProbabilities = new Dictionary<string, float>();
        float bestQvalue;
       
        foreach (Action action in currentTile.actions)
            actionProbabilities.Add(action.actionName, 1f * epsilon / currentTile.actions.Count);

        Action bestAction = Manager.instance.GetBestAction(currentTile,out bestQvalue);
        string chosenAction = bestAction.actionName;


        
        actionProbabilities[bestAction.actionName] += 1 - epsilon;

        // action = np.random.choice(np.arange(len(action_probabilities)), p = action_probabilities)

        float r = UnityEngine.Random.Range(0f, 1f);
        float sum = 0f;

        //Debug.Log("---------BEFORE SORTING-------------");
        //foreach (KeyValuePair<string, float> pair in actionProbabilities)
            //Debug.Log("ACTION NAME: " + pair.Key + " PROB: " + pair.Value);
        
        //Debug.Log("------------AFTER SORTING--------------");
        var sortedActionProbabilities = from entry in actionProbabilities orderby entry.Value ascending select entry;

        //foreach (KeyValuePair<string, float> pair in sortedActionProbabilities)
        //    Debug.Log("ACTION NAME: " + pair.Key + " PROB: " + pair.Value);
        //  actionProbabilities.OrderBy(key => key.Value


        foreach (KeyValuePair<string, float> pair in sortedActionProbabilities)
        {
          //  Debug.Log("SUM: " + sum + " RVAL: " + r);
          
            sum = sum + pair.Value;
            if (sum >= r)
            {
                chosenAction = pair.Key;
                break;
            }
        }

        foreach(Action action in actions)
        {
            if (action.actionName == chosenAction)
            {
                //if(action.actionName == bestAction.actionName)
                //    Debug.Log("(" + currentTile.state.x + " " + currentTile.state.y + ") Best Action: " + action.actionName +" RVAL: "+r);
                //else
                //    Debug.Log("(" + currentTile.state.x + " " + currentTile.state.y + ") Random Action: " + action.actionName+ " RVAL: " + r);

                return action;
            }
        }

       
        return bestAction;
    }


    void Savecsv(string []line)
    {

        string filePath = Application.dataPath + "/saved_data.csv";
        string delimiter = ",";
        int length = line.Length;
        string s = "";
        using (StreamWriter sw = File.AppendText(Application.dataPath + "/saved_data.csv"))
        {

            s = s + line[0] + "," + line[1] + "," + line[2];
            sw.WriteLine(s);
        }


    }


    public IEnumerator WaitForKeyDown(KeyCode key)
    {
        while (!Input.GetKeyDown(key))
            yield return null;
    }
    public List<Tile> GetPath(Tile startTile)
    {
        Tile currentTile = startTile;

        List<Tile> path = new List<Tile>();
        Debug.Log("GETTING THE PATH!!");
        float bestValue = 0;
        int pathCountMax = 0;
        while (currentTile.isTerminal == false && pathCountMax < 50)
        {

            Action bestAction = Manager.instance.GetBestAction(currentTile, out bestValue);
            currentTile.bestAction = bestAction;
            nextTile = bestAction.nextTile;
            path.Add(nextTile);
            currentTile = nextTile;
            pathCountMax++;
        }
        return path;

    }


}
