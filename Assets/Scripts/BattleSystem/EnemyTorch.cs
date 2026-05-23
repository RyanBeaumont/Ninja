using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;

public class EnemyTorch : EnemyCombatant
{

    public void Awake(){
        
    }


    public override void DefaultAttack()
    {
        base.DefaultAttack();
        //for each player
        foreach(var playerCombatant in BattleManager.Instance.combatants.OfType<PlayerCombatant>())
        {
            if (playerCombatant.alive)
            {
                var se = playerCombatant.HasStatusEffect("Suplex");
                if(se.amount >= 5)
                {
                    playerCombatant.RemoveStatusEffect("Suplex");
                    GameManager.Instance.ShowMessage("Torch uses SOUL SUPLEX!!!");
                    var summonAction = new EnemyAttackAction()
                    {
                        caller = this,
                        specialTarget = playerCombatant,
                        animation = "SlamAttacker",
                        receivingAnimation = "SlamVictim",
                        damage = "10",
                    };
                    BattleManager.Instance.actionQueue.Add(summonAction);
                }
            }
        }

    }

    public override float TakeDamage(Combatant caller, float baseDamage, DamageType damageType)
    {
        if(weaknesses.Contains(damageType))
        {
            GameManager.Instance.ShowMessage("!");
            //remove the weakness
            weaknesses = weaknesses.Where(w => w != damageType).ToArray();
            GameManager.Instance.ShowMessage($"{combatantName} is no longer weak to {damageType}!");
            discoveredWeaknesses.Remove(damageType);
        }
        if(damageType == DamageType.Psychic)
        {
            var randomWeakness = new DamageType[]{DamageType.Psychic,DamageType.Slashing,DamageType.Bludgeoning}[Random.Range(0,3)];
            //join the weakness to the existing weaknesses
            weaknesses = weaknesses.Concat(new DamageType[]{randomWeakness}).ToArray();
            GameManager.Instance.ShowMessage($"Your hit exposed a weakness to {randomWeakness}!");
            discoveredWeaknesses.Add(randomWeakness);
        }
        return base.TakeDamage(caller, baseDamage, damageType);
    }

    
}
