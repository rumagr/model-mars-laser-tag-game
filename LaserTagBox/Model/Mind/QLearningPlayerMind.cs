using System;
using System.Collections.Generic;
using System.Linq;
using LaserTagBox.Model.Shared;
using Mars.Interfaces.Environments;
using Mars.Numerics;
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
    
    private readonly Random _random = new();
    
    //Q-Table to store state-action values
    //tragt Flagge, Gegner in der Nähe, teammate in der Nähe, eigene Flagge in der Nähe, ExplosiveBarrel in der Nähe, Stance, actionPoints
    private double[][][][][][][][] QTable = new double[1][][][][][][][]; 
    
    public override void Init(PlayerMindLayer mindLayer)
    {
        _mindLayer = mindLayer;
        
        // Now QTable is double[1][][][][][][][] (8 dimensions)
        for (int i = 0; i < QTable.Length; i++)
        {
            QTable[i] = new double[1][][][][][][];
            for (int j = 0; j < QTable[i].Length; j++)
            {
                QTable[i][j] = new double[1][][][][][];
                for (int k = 0; k < QTable[i][j].Length; k++)
                {
                    QTable[i][j][k] = new double[1][][][][];
                    for (int l = 0; l < QTable[i][j][k].Length; l++)
                    {
                        QTable[i][j][k][l] = new double[1][][][];
                        for (int m = 0; m < QTable[i][j][k][l].Length; m++)
                        {
                            QTable[i][j][k][l][m] = new double[1][][];
                            for (int n = 0; n < QTable[i][j][k][l][m].Length; n++)
                            {
                                QTable[i][j][k][l][m][n] = new double[1][];
                                for (int o = 0; o < QTable[i][j][k][l][m][n].Length; o++)
                                {
                                    QTable[i][j][k][l][m][n][o] = new double[19];
                                }
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
        var hasFlag = Body.CarryingFlag; 
        var enemies = Body.ExploreEnemies1();
        var teammates = Body.ExploreTeam();
        var ownFlagInSight = Body.ExploreFlags2().FirstOrDefault(f => f.Team == Body.Color && f.PickedUp == false).PickedUp;
        var explosiveBarrels = Body.ExploreExplosiveBarrels1();
        var stance = Body.Stance;
        var actionPoints = Body.ActionPoints;
        //2. Choose action (exploration vs exploitation)
        var actions = QTable[hasFlag ? 1 : 0][enemies.Count > 0 ? 1 : 0][teammates.Count > 0 ? 1 : 0][ownFlagInSight ? 1 : 0][explosiveBarrels.Count > 0 ? 1 : 0][(int)stance][actionPoints].ToList();        //3. calculate reward
        
        int action = getAction(actions);
        
        if (_random.NextDouble() < explorationRate)
        {
            action = _random.Next(0, actions.Count); 
        }
        
        //3. calculate reward
        
        int reward = calculateReward();
        
        //4. Update Q-Table
        //5. save Q-Table
    }
    
    private int getAction(List<double> actions)
    {
        return actions.IndexOf(actions.Max());
    }
    
    //Q-Learning methods
    private int calculateReward()
    {
        int reward = 0;
        //TODO implement reward calculation
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
            QTable = JsonSerializer.Deserialize<double[][][][][][][][]>(File.ReadAllText(filePath));
            return true;
        }
        else
        {
            return false; 
        }
    }
    
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
    private static int goDown;
    private static int goLeft;
    private static int goRight;
}