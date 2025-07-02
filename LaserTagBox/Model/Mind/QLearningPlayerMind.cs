using System.IO;
using System.Text.Json;

namespace LaserTagBox.Model.Mind;
// author Dominik Wiesendanger, Ruben Marin Grez

public class QLearningPlayerMind : AbstractPlayerMind
{
    private PlayerMindLayer _mindLayer;
    
    //Q-Learning parameters
    private static double learningRate = 0.1;
    
    private static double discountFactor = 0.5;

    private static double explorationRate = 0.2;
    
    //Q-Table to store state-action values
    //tragt Flagge, Gegner in der Nähe, teammate in der Nähe, eigene Flagge in der Nähe, ExplosiveBarrel in der Nähe, Stance
    private double[][][][][][] QTable = new double[2][][][][][]; 
    
    public static int _teamScore = 0;
    private int _prevTeamScore = 0;
    
    private int _prevGamePoints = 0;
    
    private bool hadFlagLastTick = false; //todo: aktualisieren bei aufheben/sterben
    private bool wasAliveLastTick = true;
    
    public override void Init(PlayerMindLayer mindLayer)
    {
        _mindLayer = mindLayer;
        
        if (!LoadQTable("../../../Model/QTable.json"))
        {
            for(int m = 0; m < 2; m++)
            {
                QTable[m] = new double[2][][][][];
                for(int f = 0; f < 2; f++)
                {
                    QTable[m][f] = new double[2][][][];
                    for(int e = 0; e < 2; e++)
                    {
                        QTable[m][f][e] = new double[2][][];
                        for(int t = 0; t < 2; t++)
                        {
                            QTable[m][f][e][t] = new double[3][]; // Stance: Standing, Kneeling, Lying
                            for(int s = 0; s < 3; s++)
                            {
                                QTable[m][f][e][t][s] = new double[15]; 
                            }
                        }
                    }
                }
            }
        }
    }

    public override void Tick()
    {
        
        //TODO implement Q-Learning logic
        //1. Get current state
        //2. Choose action (exploration vs exploitation)
        //3. calculate reward
        //4. Update Q-Table
        //5. save Q-Table
    }
    
    //Q-Learning methods
    
    
    private int calculateReward()
    {
        int reward = 0;
        // Reward for assist
        if () // wenn gegner getaggt wurde (gamescore +10), nicht am äußersten rand der visual range war und verschwindet
        {
            reward += rewardAssist;
        }
        
        // 10 gamepoints fürs taggen, 20 für einen kill, -10 wenn tot
        // Alles unter 20 Game Points ist ein Tag, kein kill
        if(Body.GamePoints > _prevGamePoints && Body.GamePoints < _prevGamePoints + 20)
        {
            _prevGamePoints = Body.GamePoints;
        }
        // Reward für Kill
        if (Body.GamePoints >= _prevGamePoints + 20)
        {
            reward += rewardEnemyKilled;
            _prevGamePoints = Body.GamePoints;
        }
        
        // Reward fürs scoren
        if (hadFlagLastTick && !Body.CarryingFlag && Body.Alive && wasAliveLastTick)
        {
            _teamScore++;
            hadFlagLastTick = false;
        }

        if (_teamScore > _prevTeamScore)
        {
            reward += rewardScored;
            _prevTeamScore = _teamScore;
        }
        
        // Penalty für Tod
        if (wasAliveLastTick && Body.Alive == false)
        {
            reward += penaltyDied;
            wasAliveLastTick = false; //muss bei respawn wieder auf true gesetzt werden
            _prevGamePoints = Body.GamePoints; // weil man -10 kriegt wenn man stirbt
        }
        
        // Penalty für Enemy Scored
        if () // Ersan meinte man kann die teamscores abfragen irgendwo in battlegrounds
        {
            reward += penaltyEnemyScored;
        }
        
        // Penalty für Teammate getötet
        if () // Teammate in visual range, spieler taggt, kriegt keinen score und teammate verschiwndet obwohl er nicht am rand der visual range war
        {
            reward += penaltyTeammateKilled;
        }
        
        return reward; 
    }
    
    public void SaveQTable(string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(filePath, JsonSerializer.Serialize(QTable, options));
    }
    
    public bool LoadQTable(string filePath)
    {
        if (File.Exists(filePath))
        {
            QTable = JsonSerializer.Deserialize<double[][][][][][]>(File.ReadAllText(filePath));
            return true;
        }
        else
        {
            return false; 
        }
    }
    
    // Rewards

    private static int rewardAssist = 50;
    private static int rewardScored = 100;
    private static int rewardEnemyKilled = 50;
    
    // Penalties

    private static int penaltyDied = -10;
    private static int penaltyEnemyScored = -110;
    private static int penaltyTeammateKilled = -50;
    
    // Actions
    //private static int exploreBarriers;
    //private static int exploreDitches;
    //private static int exploreHills;
    private static int exploreEnemies;
    private static int exploreTeam;
    //private static int exploreWater;
    //private static int exploreBarrels;
    private static int exploreExplosiveBarrels;
    //private static int exploreEnemyFlagStands;
    //private static int exploreOwnFlagStand;
    private static int exploreFlags;
    //private static int hasBeeline;
    private static int getDistance;

    private static int shootEnemy;
    private static int shootExplosiveBarrell;
    private static int reload;
    
    private static int standUp;
    private static int kneel;
    private static int layDown;

    private static int goUp;
    private static int goUpRight;
    private static int goUpLeft;
    private static int goDown;
    private static int goDownRight;
    private static int goDownLeft;
    private static int goLeft;
    private static int goRight;
}