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
        
        //perform Action;
        while (Body.ActionPoints > 0 && action >= 0 && action < actions.Count)
        {
            performAction(action);
            
            //3. calculate reward
        
            int reward = calculateReward();
        
            //TODO 4. Update Q-Table 
            var qValue = QTable[hasFlag ? 1 : 0][enemies.Count > 0 ? 1 : 0][teammates.Count > 0 ? 1 : 0][ownFlagInSight ? 1 : 0][explosiveBarrels.Count > 0 ? 1 : 0][(int)stance][actionPoints][action];
        
            if (qValue < 100000 && qValue > -100000)
            {
                QTable[hasFlag ? 1 : 0][enemies.Count > 0 ? 1 : 0][teammates.Count > 0 ? 1 : 0][ownFlagInSight ? 1 : 0]
                    [explosiveBarrels.Count > 0 ? 1 : 0][(int)stance][actionPoints][action] = qValue; 
            }
            
            actions.Remove(action);
            getAction(actions); 
        }
        
        //5. save Q-Table
        SaveQTable("../../../Model/QTable.json");
    }
    
    private void performAction(int action)
    {
        switch (action)
        {
            case 0: // exploreEnemies
                Body.ExploreEnemies1();
                break;
            case 1: // exploreTeam
                Body.ExploreTeam();
                break;
            case 2: // exploreExplosiveBarrels
                Body.ExploreExplosiveBarrels1();
                break;
            case 3: // exploreFlags
                Body.ExploreFlags2();
                break;
            case 4: // shootEnemy
                var enemies = Body.ExploreEnemies1();
                if (enemies.Count > 0)
                {
                    Body.Tag5(enemies.First().Position);
                }
                break;
            case 5: // shootExplosiveBarrel
                var explosiveBarrels = Body.ExploreExplosiveBarrels1();
                if (explosiveBarrels.Count > 0)
                {
                    Body.Tag5(explosiveBarrels.First());
                }
                break;
            case 6: // reload
                Body.Reload3();
                break;
            case 7: // standUp
                Body.ChangeStance2(Stance.Standing);
                break;
            case 8: // kneel
                Body.ChangeStance2(Stance.Kneeling);
                break;
            case 9: // layDown
                Body.ChangeStance2(Stance.Lying);
                break;
            case 10: // goUp
                Body.GoTo(Position.CreatePosition(Body.Position.X , Body.Position.Y + 10)); 
                break;
            case 11: // goUpRight
                Body.GoTo(Position.CreatePosition(Body.Position.X + 10, Body.Position.Y + 10)); 
                break;
            case 12: // goUpLeft
                Body.GoTo(Position.CreatePosition(Body.Position.X - 10, Body.Position.Y + 10)); 
                break;
            case 13: // goDown
                Body.GoTo(Position.CreatePosition(Body.Position.X, Body.Position.Y - 10)); 
                break;
            case 14: // goDownRight
                Body.GoTo(Position.CreatePosition(Body.Position.X + 10, Body.Position.Y - 10)); 
                break;
            case 15: // goDownLeft
                Body.GoTo(Position.CreatePosition(Body.Position.X - 10, Body.Position.Y - 10)); 
                break;
            case 16: // goLeft
                Body.GoTo(Position.CreatePosition(Body.Position.X - 10, Body.Position.Y)); 
                break;
            case 17: // goRight
                Body.GoTo(Position.CreatePosition(Body.Position.X + 10, Body.Position.Y)); 
                break;
        }
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