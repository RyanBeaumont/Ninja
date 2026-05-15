using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class EnemyLibrarian : EnemyCombatant
{
    List<GameAction> bonusActions = new List<GameAction>();
    int books = 0;

    public void Awake(){
        bonusActions.Add(new SummonAction()
            {
                animation = "Burst",
                summon = Resources.Load<GameObject>("Enemies/EnemyBook1")
            });
        bonusActions.Add(new SummonAction()
            {
                animation = "Burst",
                summon = Resources.Load<GameObject>("Enemies/EnemyBook2")
            });
        bonusActions.Add(new SummonAction()
            {
                animation = "Burst",
                summon = Resources.Load<GameObject>("Enemies/EnemyBook3")
            });
    }


    public override void DefaultAttack()
    {
        var randomIndex = Random.Range(0, bonusActions.Count);
        var randomAction = bonusActions[randomIndex];
        randomAction.caller = this;
        BattleManager.Instance.actionQueue.Add(randomAction);
        randomAction.text = "Librarian brings a book to life!";
        base.DefaultAttack();
    }

    public void UltimateAttack()
    {
        List<EnemyCombatant> allies = BattleManager.Instance.combatants.OfType<EnemyCombatant>().Where(c => c != this).ToList();
        foreach(EnemyCombatant ally in allies)
        {
            ally.RemoveStatusEffect("");
            ally.TakeDamage(this,9999,DamageType.Psychic);
            books ++;
        }
        Attack(
            new List<EnemyAttackData>
            {
                new EnemyAttackData
                {
                    attackName = "Book Him!",
                    attackPattern = "ThrowBook",
                    damage = "40",
                    mpCost = 0,
                    hits = 4,
                    damageType = DamageType.Bludgeoning,
                    targetType = TargetType.None
                }
            }
        );
    }
}
