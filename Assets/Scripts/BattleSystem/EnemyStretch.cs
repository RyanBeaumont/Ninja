using UnityEngine;
using System.Collections.Generic;
public class EnemyStretch : EnemyCombatant
{
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
                animation = "Drink",
                statusEffect = new StatusEffect()
                {
                    name = "SpeedUp",
                    amount = 0.25f,
                    duration = 2,
                }
            });
        bonusActions.Add(new StatusEffectAction()
            {
                targetType = TargetType.SingleAlly,
                animation = "Drink",
                statusEffect = new StatusEffect()
                {
                    name = "Rock Solid",
                    stat = "DEF",
                    amount = -1f,
                    additive = true,
                    duration = -1,
                    removeOnHit = true
                }
            });
    }


    public override void DefaultAttack()
    {
        
        var randomIndex = Random.Range(0, bonusActions.Count);
        var randomAction = bonusActions[randomIndex];
        randomAction.caller = this;
        BattleManager.Instance.actionQueue.Add(randomAction);
        if(randomIndex == 0){randomAction.text = $"Stretch throws Coke at their ally, healing them";}
        if(randomIndex == 1){randomAction.text = $"Stretch throws MrSpeed at their ally, increasing their speed";}
        if(randomIndex == 2){randomAction.text = $"Stretch throws Bepsi at their ally, shielding them for one hit";}
        base.DefaultAttack();
    }
}
