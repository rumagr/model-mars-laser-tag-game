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
    
    
    
    //Q-Table to store state-action values
    //tragt Flagge, Gegner in der Nähe, teammate in der Nähe, eigene Flagge in der Nähe
    private double[][][][][] QTable = new double[2][][][][]; 
    
    public override void Init(PlayerMindLayer mindLayer)
    {
        _mindLayer = mindLayer;
        
        if (!LoadQTable("../../../Model/QTable.json"))
        {
            for (int i = 0; i < 2; i++)
            {
                QTable[i] = new double[2][][][];
                for (int j = 0; j < 5; j++)
                {
                    QTable[i][j] = new double[2][][];
                    
                    for (int k = 0; k < 5; k++)
                    {
                        QTable[i][j][k] = new double[2][];
                        for (int l = 0; l < 5; l++)
                        {
                            QTable[i][j][k][l] = new double[40]; //TODO initialize QTable with number of actions

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
            QTable = JsonSerializer.Deserialize<double[][][][][]>(File.ReadAllText(filePath));
            return true;
        }
        else
        {
            return false; 
        }
    }
}