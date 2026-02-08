using UnityEngine;
using System.Collections.Generic;

public class EnemyJade : EnemyCombatant
{

    public void PerformCounterAttack()
    {
        
        Combatant specialTarget = BattleManager.Instance.activeCombatant;
        if(HasStatusEffect("Stunned") != null)
        {
            BattleManager.Instance.actionQueue.Add(new StunAction()
            {
                caller = this,
                animation = "Defeated",
            });
            GameManager.Instance.ShowMessage($"{combatantName} is stunned and cannot move!");
            return;
        }
        List<EnemyAttackData> counterAttackData = new List<EnemyAttackData>()
        {
            new EnemyAttackData
            {
                attackName = "Counterslash",
                attackPattern = "SwordWhirlwind",
                damageType = DamageType.Slashing,
                targetType = TargetType.SingleEnemy,
                damage = "40",
            },
            new EnemyAttackData
            {
                attackName = "Counter Combo",
                attackPattern = "ThrustNSlash",
                damageType = DamageType.Slashing,
                targetType = TargetType.SingleEnemy,
                hits = 2,
                damage = "30",
            }
        };
        Attack(counterAttackData, specialTarget);
    }

    public void MarkForDeath()
    {
        if(HasStatusEffect("Counterattack") == null)
        {
            GameManager.Instance.ShowMessage("Jade regains her focus and prepares to counterattack");
            BattleManager.Instance.currentTargets = new List<Combatant>(){this};
            BattleManager.Instance.actionQueue.Add(new StatusEffectAction()
            {
                caller = this,
                animation = "ArmsCrossed",
                targetType = TargetType.Self,
                statusEffect = new StatusEffect()
                {
                    name = "Counterattack",
                    amount = 1,
                    duration = -1
                },
            });
            return;
        }

        List<EnemyAttackData> specialAttackData = new List<EnemyAttackData>()
        {
            new EnemyAttackData
            {
                attackName = "Fatal Blow",
                attackPattern = "SwordHeavy",
                damage = "200",
                mpCost = 0,
                damageType = DamageType.Slashing,
                targetType = TargetType.SingleEnemy
            },
            new EnemyAttackData
            {
                attackName = "Fatal Combo",
                attackPattern = "CrescentKick, Sweep, SpinKick",
                damage = "40",
                mpCost = 0,
                damageType = DamageType.Slashing,
                targetType = TargetType.SingleEnemy
            }
        };
        foreach(Combatant c in BattleManager.Instance.combatants)
        {
            if(c is PlayerCombatant player)
            {
                if(c.HasStatusEffect("Marked for Death") != null)
                {
                    c.RemoveStatusEffect("Marked for Death");
                    BattleManager.Instance.currentTargets = new List<Combatant>(){c};
                    //Attack
                    Attack(specialAttackData, c); //Add c to this to special-select the marked target
                    print("Attacking with killing blow");
                    return; //Stop the marking
                }
            }
        }

        //No targets are marked
        BattleManager.Instance.actionQueue.Add(new StatusEffectAction()
        {
            caller = this,
            targetType = TargetType.SingleEnemy,
            animation = "Objection",
            statusEffect = new StatusEffect()
            {
                name = "Marked for Death",
                amount = 1,
                duration = 2
            },
        });
    }
}
