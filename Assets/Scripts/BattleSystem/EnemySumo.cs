using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemySumo : EnemyCombatant
{

    public override void DefaultAttack()
    {
        base.DefaultAttack();
    }

    public override float TakeDamage(Combatant caller, float baseDamage, DamageType damageType)
    {
        float dmg = base.TakeDamage(caller, baseDamage, damageType);
        ApplyStatusEffect(CardDatabase.Instance.getStatusEffect("Armor",10,1));
        GameManager.Instance.ShowMessage($"Hitting {combatantName} makes their armor tougher!");
        return dmg;
    }



}
