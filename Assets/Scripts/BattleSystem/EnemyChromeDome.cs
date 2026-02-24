using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class EnemyChromeDome : EnemyCombatant
{
    public bool reinforcements = false;
    int reinforcementTurn = 0;
    List<GameAction> bonusActions = new List<GameAction>();

    public void Awake(){
        bonusActions.Add(new HealAction()
            {
                targetType = TargetType.SingleAlly,
                animation = "Drink",
                healAmount = "50"
            });
        bonusActions.Add(new StatusEffectAction()
            {
                targetType = TargetType.SingleAlly,
                animation = "Throw",
                statusEffect = new StatusEffect()
                {
                    name = "SpeedUp",
                    amount = 0.25f,
                    duration = 2,
                }
            });
        bonusActions.Add(new StatusEffectAction()
            {
                targetType = TargetType.SingleEnemy,
                animation = "Throw",
                statusEffect = new StatusEffect()
                {
                    name = "Off-Balance",
                    stat = "DEF",
                    amount = 0.25f,
                    duration = -1,
                    removeOnHit = true
                }
            });
    }


    public override void DefaultAttack()
    {
        weaknesses = new DamageType[]{};
        resistances = new DamageType[]{DamageType.Slashing,DamageType.Bludgeoning,DamageType.Psychic};
        base.DefaultAttack();

        reinforcementTurn ++;
        if (reinforcements)
        {
            if(reinforcementTurn == 2)
            {
                List<GameObject> enemyPrefabs = new List<GameObject>()
                {
                    Resources.Load<GameObject>("Enemies/SpartanSwordsman"),
                    Resources.Load<GameObject>("Enemies/SpartanSwordsman"),
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
            if(reinforcementTurn == 4)
            {
                List<GameObject> enemyPrefabs = new List<GameObject>()
                {
                    Resources.Load<GameObject>("Enemies/SpartanGreenBelt"),
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
            if(reinforcementTurn == 8)
            {
                List<GameObject> enemyPrefabs = new List<GameObject>()
                {
                    Resources.Load<GameObject>("Enemies/SpartanSwordsman"),
                    Resources.Load<GameObject>("Enemies/SpartanSwordsman"),
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
        /*
        var randomAction = bonusActions[Random.Range(0, bonusActions.Count)];
        randomAction.caller = this;
        BattleManager.Instance.actionQueue.Add(randomAction);
        GameManager.Instance.ShowMessage($"{combatantName} drinks a random soda!");
       */
    }
     

    public void UltimateAttack()
    {
        weaknesses = new DamageType[]{DamageType.Slashing,DamageType.Bludgeoning,DamageType.Psychic};
        resistances = new DamageType[]{};
        var specialAttack = 
            new EnemyAttackData
            {
                attackName = "NO MERCY and throws himself off balance!",
                attackPattern = "SwordWhirlwind, Sweep, Uppercut, Punch",
                damage = "20",
                mpCost = 0,
                damageType = DamageType.Bludgeoning,
                targetType = TargetType.AllEnemies
            };
        Attack(new List<EnemyAttackData>(){specialAttack});
    }
}
