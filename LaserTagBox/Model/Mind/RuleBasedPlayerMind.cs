using System.Linq;
using LaserTagBox.Model.Shared;
using Mars.Common.Core.Random;
using Mars.Interfaces.Environments;
using Mars.Numerics;
using ServiceStack;

namespace LaserTagBox.Model.Mind;

public class RuleBasedPlayerMind : AbstractPlayerMind
{
    private Position _enemyFlagStand;
    private PlayerMindLayer _mindLayer;
    private Position _goal;

    public override void Init(PlayerMindLayer mindLayer)
    {
        _mindLayer = mindLayer;
    }

    public override void Tick()
    {
        if (Body.RemainingShots == 0)
        {
            Body.Reload3(); // Reload if no shots left
        }
        
        var enemies = Body.ExploreEnemies1();
        if (enemies != null && enemies.Count > 0)
        {
            Stance newStance = Stance.Standing;
            
            if (Body.GetDistance(enemies.FirstOrDefault().Position) > 8)
            {
                newStance = Stance.Standing;    
            }
            else if (Body.GetDistance(enemies.FirstOrDefault().Position) <= 8 && Body.GetDistance(enemies.FirstOrDefault().Position) > 5)
            {
                newStance = Stance.Kneeling; 
            }
            else if (Body.GetDistance(enemies.FirstOrDefault().Position) <= 5)
            {
                newStance = Stance.Lying;
            }

            if (newStance != Body.Stance)
            {
                Body.ChangeStance2(newStance);
            }

            var explosiveBarrelPositions = Body.ExploreExplosiveBarrels1();

            var shotFired = false; 
            
            for (int i = 0; i < explosiveBarrelPositions.Count; i++)
            {
                if (Body.GetDistance(explosiveBarrelPositions[i]) > 3 && Distance.Euclidean(explosiveBarrelPositions[i].X,explosiveBarrelPositions[i].Y, enemies.FirstOrDefault().Position.X,enemies.FirstOrDefault().Position.Y) <= 3)
                {
                    Body.Tag5(explosiveBarrelPositions[i]);
                    shotFired = true;
                    break; 
                }
            }
            
            if (!shotFired)
            {
                Body.Tag5(enemies.FirstOrDefault().Position); // Tag the first enemy
            }

            if (Body.RemainingShots <= 3)
            {
                Body.Reload3(); // Reload if shots are low
            }
            
            flagColletion(); // Move towards the flag
        }
        
    }

    private void flagColletion()
    {
        _enemyFlagStand ??= Body.ExploreEnemyFlagStands1()[0];
        if (Body.ActionPoints > 2 && !Body.CarryingFlag)
        {
            var flags = Body.ExploreFlags2();
            var ownFlag = flags.FirstOrDefault(f => f.Team == Body.Color);
            var ownFlagStand = Body.ExploreOwnFlagStand();
            if (Distance.Euclidean(ownFlagStand.X, ownFlagStand.Y, ownFlag.Position.X, ownFlag.Position.Y) > 2)
            {
                _goal = ownFlag.Position;
            }
            else
            { 
                _goal = flags.Where(f => f.Team != Body.Color && f.PickedUp == false).Select(f => f.Position).FirstOrDefault();
            }
        }
        if (Body.CarryingFlag)
        {
            var flagStand = Body.ExploreOwnFlagStand();
            _goal = flagStand;
        }
        
        if (_goal == null)
        {
            _goal = Body.Position;
        }
        var moved = Body.GoTo(_goal);
    }
    
}