using System.Collections.Generic;
using UnityEngine;

[System.Serializable]public class StatusEffect
{
    public string name;
    public string stat;
    public float amount;
    public bool additive = true; //true = additive, false = multiplicative
    public int duration = -1; //-1 = permanent
    public bool removeOnHit = false;
}

public enum DamageType
{
    Slashing, Bludgeoning, Psychic
}
class TurnPreviewEntry
{
    public Combatant combatant;
    public float initiative;
}

public class GameAction
{
    public Combatant caller;
    //public List<Combatant> targets;
    public TargetType targetType;
    public string animation;
    public string text;
    public int bonusActions = 0;
    public virtual void Execute(BattleManager battleManager)
    {
        caller.PlayAnimation(animation);
    }
}

public class ChooseTargetsAction : GameAction
{
    public string prompt;
    public GameAction gameAction; //action to perform after targeting
    public bool targetDead = false;

    public override void Execute(BattleManager battleManager)
    {
        Targeter targeter = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Targeter")).GetComponent<Targeter>();
        targeter.Initialize(targetType, prompt, gameAction, targetDead);
        battleManager.waitingForInput = true;
    }
}

public class EnemyAttackAction : GameAction
{
    public string damage;
    public int hits;
    public DamageType damageType;
    public Combatant specialTarget = null;
    public StatusEffect statusEffect = null;
    public float timeScale = 0.25f;
    public bool loopAnimation = true;

    public override void Execute(BattleManager battleManager)
    {
        AudioManager.Instance.PlaySoundEffect("s_dbz_jump",UnityEngine.Random.Range(0.8f,1.2f));
        if(specialTarget != null){
            battleManager.SelectTargets(new List<Combatant>(){specialTarget});
            Transform spawnPoint = GameObject.Find("BattleSetup/PlayerSpawn").transform;
            battleManager.SetPose(specialTarget.transform, "", CameraAngle.behind, "");
        }
        else
        {
            battleManager.SelectRandomTargets(battleManager.activeCombatant, targetType);
        }
            
         battleManager.waitingForInput = true; //wait for animation input
        caller.PlayAnimation(animation);
        battleManager.hitsRemaining = hits;
        battleManager.pendingDamage = caller.EvaluateStatFormula(damage);
        Time.timeScale = timeScale; //slow down time for dramatic effect
        battleManager.pendingDamageType = damageType;
        if(statusEffect != null && statusEffect.name != "")
            battleManager.pendingStatusEffect = statusEffect;
        battleManager.canDodge = true;
        battleManager.loopAnimation = loopAnimation;
        
    }
}

public class StabbyStabAction: DamageAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        battleManager.hitsRemaining = battleManager.hitCounter + 1;
        GameManager.Instance.ShowMessage($"Stabbing {battleManager.hitCounter + 1} times");
    }
}

public class NullifyDamageAction : DamageAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        foreach(var t in battleManager.currentTargets)
        {
            t.RemoveStatusEffect("");
            t.ApplyStatusEffect(new StatusEffect()
            {
                name = "Off-Balance",
                stat = "DEF",
                amount = .25f,
                duration = -1,
                removeOnHit = true
            });
        }
        
    }
}

public class NullifyDamageAction2 : DamageAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        foreach(var t in battleManager.currentTargets)
        {
            t.RemoveStatusEffect("");
        }
    }
}

public class DamageAction : GameAction
{
    public string damage;
    public int hits;
    public string receivingAnimation = "";
    public DamageType damageType;
    public StatusEffect statusEffect = null;
    public bool loopAnimation = false;

    public override void Execute(BattleManager battleManager)
    {
        AudioManager.Instance.PlaySoundEffect("s_dbz_jump",UnityEngine.Random.Range(0.8f,1.2f));
        battleManager.waitingForInput = true; //wait for animation input
        battleManager.hitsRemaining = hits;
        battleManager.pendingDamage = caller.EvaluateStatFormula(damage);
        battleManager.pendingDamageType = damageType;
        battleManager.pendingStatusEffect = statusEffect;
        battleManager.loopAnimation = loopAnimation;
        caller.PlayAnimation(animation);
        if(receivingAnimation != ""){foreach(Combatant c in battleManager.currentTargets) c.PlayAnimation(receivingAnimation);}
    }
}

public class SelfDamageAction : GameAction
{
    public string damage;
    public DamageType damageType;
     public override void Execute(BattleManager battleManager)
    {
        caller.PlayAnimation(animation);
        caller.TakeDamage(caller,caller.EvaluateStatFormula(damage),damageType);
    }

}

public class ChiBladeAction : DamageAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        caller.mp = 0;
    }
}

public class BattleOfWillsAction : DamageAction
{
    public override void Execute(BattleManager battleManager)
    {
        damage = $"{(caller.mp - battleManager.currentTargets[0].mp)}";
        base.Execute(battleManager);
    }
}

public class SuplexDamageAction : DamageAction
{
    public override void Execute(BattleManager battleManager)
    {
        if(battleManager.currentTargets.Count == 1 && (battleManager.currentTargets[0].HasStatusEffect("Off-Balance")!=null || battleManager.currentTargets[0].HasStatusEffect("Prone") != null))
        {
            battleManager.currentTargets[0].RemoveStatusEffect("Off-Balance");
            battleManager.currentTargets[0].ApplyStatusEffect(new StatusEffect
            {
                name = "Prone",
                amount = 1,
                duration = 2,
            });
            damage = "60";
        }
        caller.ApplyStatusEffect(new StatusEffect()
        {
            name = "Off-Balance",
            stat = "DEF",
            amount = .25f,
            duration = -1,
            removeOnHit = true
        });
        base.Execute(battleManager);
    }
}

public class GrappleDamageAction : DamageAction
{
    public bool lifesteal = false;
    public override void Execute(BattleManager battleManager)
    {
        if(battleManager.currentTargets.Count == 1 && (battleManager.currentTargets[0].HasStatusEffect("Off-Balance")!=null || battleManager.currentTargets[0].HasStatusEffect("Prone") != null) || lifesteal)
        {
            battleManager.currentTargets[0].RemoveStatusEffect("Off-Balance");
            battleManager.currentTargets[0].ApplyStatusEffect(new StatusEffect
            {
                name = "Prone",
                amount = 1,
                duration = 2,
            });
            statusEffect = new StatusEffect
            {
                name = "Stunned",
                amount = 1,
                duration = 1,
            };
            if (lifesteal)
            {
                battleManager.currentTargets[0].ApplyStatusEffect(new StatusEffect()
                {
                    name = "Lifesteal",
                    amount = 1,
                    additive = true,
                    duration = 2,
                });
            }
        }
        caller.ApplyStatusEffect(new StatusEffect()
        {
            name = "Off-Balance",
            stat = "DEF",
            amount = .25f,
            duration = -1,
            removeOnHit = true
        });
        base.Execute(battleManager);
    }
}

public class WildSwingAction : GameAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        if(caller is PlayerCombatant playerCombatant)
        {
            Card newCard  = playerCombatant.Scry();
            battleManager.ExecuteCard(newCard,caller);
            GameManager.Instance.ShowMessage($"{caller.combatantName} wild swings into {newCard.cardName}!");
            playerCombatant.deck.RemoveAt(0);
            playerCombatant.discard.Add(newCard);
        }
    }
}

public class HealAction : GameAction
{
    public string healAmount;

    public override void Execute(BattleManager battleManager)
    {
        caller.PlayAnimation(animation);
        foreach(var t in battleManager.currentTargets)
        {
            t.Heal(caller.EvaluateStatFormula(healAmount));
        }
    }
}

public class ReviveAction : HealAction
{
    public override void Execute(BattleManager battleManager)
    {
        foreach(var t in battleManager.currentTargets)
        {
            t.alive = true;
        }
        base.Execute(battleManager);
    }
}

public class ShareTPAction : HealAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        if(caller is PlayerCombatant callerPlayer){
        foreach(var t in battleManager.currentTargets)
        {
            if(t is PlayerCombatant player) player.GainTP(callerPlayer.tp);
            callerPlayer.tp = 0;
        }
        }
    }
}

public class StatusEffectAction : GameAction
{
    public StatusEffect statusEffect;

    public override void Execute(BattleManager battleManager)
    {
        caller.PlayAnimation(animation);
        if(caller is EnemyCombatant e){battleManager.SelectRandomTargets(caller,targetType);}
        foreach(var t in battleManager.currentTargets)
        {
            t.ApplyStatusEffect(statusEffect);
        }
    }
}

public class LockInAction : StatusEffectAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        foreach(var t in battleManager.currentTargets)
        {
            t.ApplyStatusEffect(new StatusEffect()
            {
                name = "Weakened",
                stat = "DEF",
                amount = 1.5f,
                additive = true,
                duration = 3,

            });
            t.ApplyStatusEffect(new StatusEffect()
            {
                name = "Locked In",
                stat = "PSY",
                amount = 2,
                additive = false,
                duration = 3,

            });
        }
    }
}

public class StunAction : GameAction
{
    public override void Execute(BattleManager battleManager){
        base.Execute(battleManager);
    }
}

public class LifestrikeAction : DamageAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        battleManager.lifestrike = true;
    }
}

public class DrawCardsAction : GameAction
{
    public int cardCount;

    public override void Execute(BattleManager battleManager)
    {
        caller.PlayAnimation(animation);
        if (caller is PlayerCombatant player)
        {
            player.DrawCards(cardCount);
            GameManager.Instance.ShowMessage($"{caller.combatantName} draws {cardCount} cards!");
        }
    }
}

public class CardExchangeAction : GameAction
{

    public override void Execute(BattleManager battleManager)
    {
        caller.PlayAnimation(animation);
        if (caller is PlayerCombatant player)
        {
            player.DrawCards(battleManager.discardPower + 1);
            GameManager.Instance.ShowMessage($"{caller.combatantName} draws {battleManager.discardPower + 1} cards!");
        }
    }
}

public class DrawUntilAction : DrawCardsAction
{
    public override void Execute(BattleManager battleManager)
    {
        caller.PlayAnimation(animation);
        int cards = 0;
        if (caller is PlayerCombatant player)
        {
            while(player.hand.Count < cardCount){
                player.DrawCards(cardCount);
                cards ++;
            }
            GameManager.Instance.ShowMessage($"{caller.combatantName} draws {cards} cards!");
        }
    }
}

public class GainMPAction : GameAction
{
    public string mpAmount;

    public override void Execute(BattleManager battleManager)
    {
        caller.PlayAnimation(animation);
        caller.GainMP(caller.EvaluateStatFormula(mpAmount));
    }
}

public class ReloadAction : GameAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        foreach(Combatant c in battleManager.currentTargets)
        {
            if(c is PlayerCombatant player && player.discard.Count > 0)
            {
                var card = player.discard[0];
                card.tempCost = Mathf.Max(0,card.cost - 20);
                player.discard.RemoveAt(0);
                player.deck.Insert(0,card);
                player.DrawCards(1);
                 GameManager.Instance.ShowMessage($"{player.combatantName} re-draws {card.cardName}");
            }
            else
            {
                GameManager.Instance.ShowMessage("Discard is empty - nothing to reload");
            }
        }
    }
}

public class ReduceCostAction : DamageAction
{
    public int amount;

    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        if(caller is PlayerCombatant p)
        {
            foreach(Card card in p.hand)
            {
                if(card.tempCost != 0)
                card.tempCost = Mathf.Max(0,card.tempCost - 20);
                else
                card.tempCost = Mathf.Max(0,card.cost - 20);
            }
            GameObject.FindFirstObjectByType<HandManager>().UpdateHandVisuals();
            GameManager.Instance.ShowMessage("Card cost reduced!");
        }
        
    }
}

public class EnergySuckAction : GameAction
{
    public string mpAmount;

    public override void Execute(BattleManager battleManager)
    {
        if(battleManager.currentTargets[0] is PlayerCombatant p)
        {
            p.GainMP(caller.EvaluateStatFormula(mpAmount));
            GameManager.Instance.ShowMessage($"{caller.combatantName} winks at {p.combatantName}. You got this, girl! +{caller.EvaluateStatFormula(mpAmount)} MP");
        }
        else
        {
            float suckAmount = Mathf.Min(caller.EvaluateStatFormula(mpAmount), battleManager.currentTargets[0].mp);
            battleManager.currentTargets[0].GainMP(-caller.EvaluateStatFormula(mpAmount));
            caller.GainMP(suckAmount);
            GameManager.Instance.ShowMessage($"{caller.combatantName} winks at {battleManager.currentTargets[0].combatantName}, stealing their heart and {suckAmount} MP");

        }
        caller.PlayAnimation(animation);
    }
}