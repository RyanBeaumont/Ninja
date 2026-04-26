using UnityEngine;

public class VengefulSword : EnemyCombatant
{
    Combatant target = null;

    public override float TakeDamage(Combatant caller, float baseDamage, DamageType damageType)
    {
        if(caller is PlayerCombatant){
            target = caller;
            GameManager.Instance.ShowMessage($"<color=yellow>{combatantName} now wants revenge on {caller.combatantName} </color>");
        }
        return base.TakeDamage(caller, baseDamage, damageType);
    }

    public override void DefaultAttack()
    {
        Attack(null,target);
    }
}
