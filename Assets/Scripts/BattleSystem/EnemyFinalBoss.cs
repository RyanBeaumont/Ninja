using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemyFinalBoss : EnemyCombatant
{

    public override void DefaultAttack()
    {
        resistances = new DamageType[]{};
        weaknesses = new DamageType[]{DamageType.Slashing,DamageType.Bludgeoning,DamageType.Psychic};
        discoveredWeaknesses = new List<DamageType>();
        discoveredResistances.Clear();
        base.DefaultAttack();
    }

    public override float TakeDamage(Combatant caller, float baseDamage, DamageType damageType)
    {
        float dmg = base.TakeDamage(caller, baseDamage, damageType);
        if(damageType != DamageType.None){
            if(weaknesses.Contains(damageType))
            {
                //remove the weakness
                weaknesses = weaknesses.Where(w => w != damageType).ToArray();
                discoveredWeaknesses.Remove(damageType);
            }
            if (!resistances.Contains(damageType))
            {
                //Add the resistance
                resistances = resistances.Concat(new DamageType[]{damageType}).ToArray();
                discoveredResistances.Add(damageType);
            }
        
        
            if(mp >= maxMp){
                var action = new SelfDamageAction()
                {
                    caller = this,
                    animation = "Burst",
                    targetType = TargetType.Self,
                    damage = "75",
                    damageType = DamageType.Psychic,
                    text = "Jade overloads with mana, taking psychic damage!"
                };
                BattleManager.Instance.actionQueue.Add(action);
                mp = maxMp-5;
            }
            else
            {
                var action = new GainMPAction()
                {
                    caller = this,
                    animation = "LibrarianIdle",
                    targetType = TargetType.Self,
                    mpAmount = "5",
                    text = "Jade absorbs mana and learns to resist that damage type!"
                };
                BattleManager.Instance.actionQueue.Add(action);
            }
        }

        return dmg;
    }


    public void ManaDrain()
    {
        BattleManager.Instance.actionQueue.Add(new EnergySuckAction()
        {
            caller = this,
            animation = "GatherChi",
            targetType = TargetType.AllEnemies,
            mpAmount = "80",
        });
    }

    public void SummonMinions()
    {
        List<GameObject> enemyPrefabs = new List<GameObject>()
        {
            Resources.Load<GameObject>("Enemies/EnemyShadow"),
            Resources.Load<GameObject>("Enemies/EnemyShadow2"),
        };
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            var summonAction = new SummonAction()
            {
                targetType = TargetType.None,
                animation = "Summon",
                summon = enemyPrefabs[i]
            };
            BattleManager.Instance.actionQueue.Add(summonAction);
        }
    }
}
