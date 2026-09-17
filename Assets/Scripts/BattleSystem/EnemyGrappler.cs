using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemyGrappler : EnemyCombatant
{

    public void Awake(){
        
    }


    public override void DefaultAttack()
    {
        base.DefaultAttack();
    }

    public void SoulSuplex()
    {
        GameManager.Instance.ShowMessage($"{combatantName} uses SOUL SUPLEX!!!");
        var attackAction = new SoulSuplexAction()
        {
            caller = this,
            specialTarget = BattleManager.Instance.combatants.FirstOrDefault(p => p.alive && p is PlayerCombatant),
            targetType = TargetType.SingleEnemy,
            animation = "SlamAttacker",
            receivingAnimation = "SlamVictim",
            damage = "10",
        };
        BattleManager.Instance.actionQueue.Add(attackAction);
    }


    
}
