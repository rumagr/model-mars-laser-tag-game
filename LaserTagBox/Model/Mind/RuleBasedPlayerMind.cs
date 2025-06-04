using System.Linq;
using Mars.Common.Core.Random;
using Mars.Interfaces.Environments;
using Mars.Numerics;
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
        
        if (Body.ActionPoints < 10)
        {
            return;  //TODO execution order fix
        }
        var enemies = Body.ExploreEnemies1();
        if (enemies.Any())
        {
            _goal = enemies.First().Position.Copy();
            if (Body.RemainingShots == 0) Body.Reload3();
            Body.Tag5(enemies.First().Position);
        }
            
        if (_goal == null || Body.GetDistance(_goal) == 1)
        {
            var newX = RandomHelper.Random.Next(_mindLayer.Width);
            var newY = RandomHelper.Random.Next(_mindLayer.Height);
            _goal = Position.CreatePosition(newX, newY);
        }

        var moved = Body.GoTo(_goal);
        if (!moved) _goal = null;
     
    }

    private void goToFlag()
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