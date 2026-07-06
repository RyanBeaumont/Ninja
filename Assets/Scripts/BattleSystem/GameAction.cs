using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]public class StatusEffect
{
    public string name;
    public string stat;
    public Sprite sprite;
    public float amount;
    public string description;
    public bool additive = true; //true = additive, false = multiplicative
    public int duration = -1; //-1 = permanent
    public bool removeOnHit = false;
    public Combatant caller;
    public StatusUpdate statusUpdate = StatusUpdate.TurnStart;
    public GameObject particleEffect = null;
}

public enum StatusUpdate{TurnStart,TurnEnd,CallerTurnStart}  
public enum DamageType
{
    Slashing, Bludgeoning, Psychic, None
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
    public string pattern = "";
    public string text = "";
    public int bonusActions = 0;
    public bool wildSwing = false;

    protected void ResolveInsanityTargets(BattleManager battleManager)
    {
        
        if (caller != null && (caller.HasStatusEffect("Insanity") != null || caller.HasStatusEffect("Drunk") != null ) &&
            targetType != TargetType.None && targetType != TargetType.Self &&
            !(this is ChooseTargetsAction))
        {
            battleManager.SelectRandomTargets(caller, targetType);
        }
        
    }

    public virtual void Execute(BattleManager battleManager)
    {
        if (caller != null && !caller.alive) return;
        ResolveInsanityTargets(battleManager);
        caller.PlayAnimation(animation);
        if(text != ""){GameManager.Instance.ShowMessage(text);}
    }
}

public class UltimateAction : GameAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        AudioManager.Instance.PlaySoundEffect("Thunder");
        battleManager.clock = 1f;
        battleManager.SetPose(caller.transform, "", CameraAngle.super, "Mad");
        caller.PlayAnimation(animation);
        battleManager.ShowBackground();
    }
}
public class CutAction : GameAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        AudioManager.Instance.PlaySoundEffect("Battle");
        battleManager.clock = 1.5f;
        battleManager.SetPose(caller.transform, "", CameraAngle.highAngle, "Mad");
        caller.PlayAnimation("Cut");
        battleManager.ShowBackground();
    }
}

public class ChooseTargetsAction : GameAction
{
    public string prompt;
    public GameAction gameAction; //action to perform after targeting
    public bool targetDead = false;

    public override void Execute(BattleManager battleManager)
    {
        if(gameAction is ReviveAction) targetDead = true;
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
    public string receivingAnimation = "";
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
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
        
        battleManager.pendingDamageType = damageType;
        if(statusEffect != null && statusEffect.name != "")
            battleManager.pendingStatusEffect = CardDatabase.Instance.getStatusEffect(statusEffect.name,statusEffect.amount,statusEffect.duration);
        
        if(receivingAnimation != ""){foreach(Combatant c in battleManager.currentTargets) c.PlayAnimation(receivingAnimation); timeScale = 1f;}
        else{battleManager.canDodge = true;}

        Time.timeScale = timeScale; //slow down time for dramatic effect
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
            t.ApplyStatusEffect(CardDatabase.Instance.getStatusEffect("Off-Balance"));
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
            t.Heal(25f);
        }
    }
}

public class DamageAction : GameAction
{
    public string damage;
    public int hits;
    public string receivingAnimation = "";
    public DamageType damageType = DamageType.None;
    public StatusEffect statusEffect = null;
    public bool loopAnimation = false;
    public bool multiDamageType = false;

    public override void Execute(BattleManager battleManager)
    {
         battleManager.SetPose(caller.transform, "", CameraAngle.behind, "Mad");
        base.Execute(battleManager);
        AudioManager.Instance.PlaySoundEffect("s_dbz_jump",UnityEngine.Random.Range(0.8f,1.2f));
        battleManager.waitingForInput = true; //wait for animation input
        battleManager.hitsRemaining = hits;
        battleManager.pendingDamage = caller.EvaluateStatFormula(damage);
        Debug.Log($"Base damage: {caller.EvaluateStatFormula(damage)}");
        battleManager.pendingDamageType = damageType;
        if(statusEffect != null)
            statusEffect.caller = caller;
         battleManager.pendingStatusEffect = statusEffect;
        battleManager.loopAnimation = loopAnimation;
        battleManager.multiDamageType = multiDamageType;
        if(receivingAnimation != ""){foreach(Combatant c in battleManager.currentTargets) c.PlayAnimation(receivingAnimation);}
    }
}


public class CounterDamageAction : DamageAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        //battleManager.ShowBackground();
        battleManager.SetPose(caller.transform, "SwordCounter", CameraAngle.counter, "Mad");
    }
}

public class SelfDamageAction : GameAction
{
    public string damage;
    public DamageType damageType;
     public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        battleManager.clock = 2.1f;
        caller.TakeDamage(caller,caller.EvaluateStatFormula(damage),damageType);
    }

}

public class ChiBladeAction : DamageAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        if(caller.HasStatusEffect("Insanity") != null && battleManager.actionQueue.Count > 0)
        {
            
        }else{
            caller.mp = 0;
        }
    }
}

public class BattleOfWillsAction : DamageAction
{
    public override void Execute(BattleManager battleManager)
    {
        damage = $"{(caller.mp - battleManager.currentTargets[0].mp) * 2}";
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
            battleManager.currentTargets[0].ApplyStatusEffect(CardDatabase.Instance.getStatusEffect("Prone"));
        }
        caller.ApplyStatusEffect(CardDatabase.Instance.getStatusEffect("Off-Balance"));
        base.Execute(battleManager);
    }
}

public class OmnisweepDamageAction : DamageAction
{
    public override void Execute(BattleManager battleManager)
    {
        for (int i = battleManager.currentTargets.Count - 1; i >= 0; i--)
        {
            var target = battleManager.currentTargets[i];
            //Remove target if they don't have any effects
            if (target.HasStatusEffect("Off-Balance") == null && target.HasStatusEffect("Prone") == null && target.HasStatusEffect("Stunned") == null)
            {
                battleManager.currentTargets.RemoveAt(i);
            }
        }
        base.Execute(battleManager);
    }
}

public class GrappleDamageAction : DamageAction
{
    public bool lifesteal = false;
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
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
                duration = -1,
            };
            if (lifesteal)
            {
                battleManager.currentTargets[0].ApplyStatusEffect(CardDatabase.Instance.getStatusEffect("Lifesteal"));
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
        
    }
}

public class NardbusterDamageAction : DamageAction
{
    public bool lifesteal = false;
    public override void Execute(BattleManager battleManager)
    {
        if(battleManager.currentTargets.Count == 1 && (battleManager.currentTargets[0].HasStatusEffect("Off-Balance")!=null || battleManager.currentTargets[0].HasStatusEffect("Prone") != null) || lifesteal)
        {
            battleManager.currentTargets[0].RemoveStatusEffect("Off-Balance");
            battleManager.currentTargets[0].ApplyStatusEffect(CardDatabase.Instance.getStatusEffect("Prone"));
            statusEffect = CardDatabase.Instance.getStatusEffect("Stunned");
            if (lifesteal)
            {
                battleManager.currentTargets[0].ApplyStatusEffect(CardDatabase.Instance.getStatusEffect("Lifesteal"));
            }
        }
        caller.ApplyStatusEffect(CardDatabase.Instance.getStatusEffect("Off-Balance"));
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
            Card newCard  = playerCombatant.Scry(1)[0];
            foreach(GameAction a in newCard.effects)
            {
                a.wildSwing = true;
            }
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
        base.Execute(battleManager);
        foreach(var t in battleManager.currentTargets)
        {
            t.Heal(caller.EvaluateStatFormula(healAmount));
            GameManager.Instance.ShowMessage($"{t.combatantName} heals +{caller.EvaluateStatFormula(healAmount)} HP");
        }
    }
}

public class ReviveAction : GameAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        foreach(var t in battleManager.currentTargets)
        {
            if(t is PlayerCombatant p){p.Revive();}
        }
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
        base.Execute(battleManager);
        statusEffect.caller = caller;
        if(caller is EnemyCombatant e){
            Debug.Log($"Selecting random targets for status effect");
            battleManager.SelectRandomTargets(caller,targetType);}
        foreach(var t in battleManager.currentTargets)
        {
            t.ApplyStatusEffect(statusEffect);
        }
    }
}

public class SoulSuplexAction : EnemyAttackAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        if(specialTarget != null)
        {
            var myName = specialTarget.combatantName;
            var summonAction = new SummonAction()
            {
                enemy = true,
                summon = Resources.Load<GameObject>($"Enemies/EnemySoulContainer"),
                name = myName
            };
            battleManager.actionQueue.Add(summonAction);
            battleManager.RemoveCombatant(specialTarget);
            GameObject.Destroy(specialTarget,2f);
        }
    }
}


public class SummonAction : GameAction
{
    public bool enemy = true;
    public GameObject summon;
    public string name = "";

    public override void Execute(BattleManager battleManager)
    {
        //base.Execute(battleManager);
        GameObject combatantObject = null;
        if(enemy)
            combatantObject = Object.Instantiate(summon, GameObject.Find("BattleSetup/EnemySpawn").transform);
        else
            combatantObject = Object.Instantiate(summon, GameObject.Find("BattleSetup/PlayerSpawn").transform);
        //spread out combatants centered around spawn point

        int enemyCount = 0;
        if(enemy)
            enemyCount = battleManager.combatants.Count(c => c is EnemyCombatant && c.alive);
        else
            enemyCount = battleManager.combatants.Count(c => c is PlayerCombatant);

        combatantObject.transform.localPosition = new Vector3((-0.5f * YourParty.instance.spacing * enemyCount) + (YourParty.instance.spacing * enemyCount), 0f, 0f);
        combatantObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        var combatant = combatantObject.GetComponent<Combatant>();
        combatant.initiative = 50f;
        combatant.surprise = true;
        combatant.combatantName = name != "" ? name : combatant.combatantName;
        var healthbar = Object.Instantiate(Resources.Load<GameObject>("Health"), combatantObject.transform);
        if (combatant is PlayerCombatant playerCombatant)
        {
            playerCombatant.hpBar = healthbar;
        }
        BattleManager.Instance.AddCombatant(combatant);
        GameManager.Instance.ShowMessage($"<color=red>{combatant.combatantName} appears!</color>");
        var effect = Object.Instantiate(Resources.Load<GameObject>("Particles/Encounter"), combatantObject.transform);
        combatant.enabled = true;
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
                amount = 1.75f,
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
        caller.RemoveStatusEffect("Stunned");
    }
}

public class VanishAction : GameAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        if (caller is PlayerCombatant p)p.hidden = true;
        caller.SetTargetPosition(caller.transform.position + Vector3.down*4f);
        battleManager.RemoveCombatant(caller);
        AudioManager.Instance.PlaySoundEffect("Teleport");
        GameManager.Instance.ShowMessage($"{caller.combatantName} vanishes... Press [E] on any turn to reappear");
    }
}

public class ExploitWeaknessAction : DamageAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        if(battleManager.currentTargets[0] != null && battleManager.currentTargets[0].statusEffects.Count > 0)
        {
            GameManager.Instance.ShowMessage("Exploiting weakness! Bonus action gained!");
            BattleManager.Instance.attacksRemaining += 1;
        }
    }
}

public class SpeedBoostAction : GameAction
{
    //Forces the target to go next in the turn order
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        foreach(var t in battleManager.currentTargets)
        {
            t.initiative = battleManager.combatants.Max(c => c.initiative) + 1f;
            t.surprise = true;
            GameManager.Instance.ShowMessage($"{t.combatantName} is super fast now!");
        }
    }
}

public class ScryAction : GameAction
{
    public int scryAmount = 3;
    public override void Execute(BattleManager battleManager)
    {
        ResolveInsanityTargets(battleManager);
        battleManager.ShowScryPanel((PlayerCombatant)battleManager.currentTargets[0], scryAmount);
    }
}

public class ChainKillAction : GameAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        if(battleManager.currentTargets[0] != null && battleManager.currentTargets[0].hp <= 50f)
        {
            battleManager.currentTargets[0].TakeDamage(caller,100,DamageType.Slashing);
            if(battleManager.activePlayer != null)
            {
                var card = battleManager.activePlayer.discard[0];
                if(card != null) battleManager.activePlayer.discard.Remove(card);
                battleManager.activePlayer.deck.Insert(0,card);
                battleManager.activePlayer.DrawCards(1);
                GameManager.Instance.ShowMessage("Chain Kill!");
            }
            bonusActions += 1;
        }
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

public class CloserAction : DamageAction
{
    public override void Execute(BattleManager battleManager)
    {
        damage = $"30 + {25*battleManager.discardPower}*MED";
        base.Execute(battleManager);
    }
}

public class DrawUntilAction : DrawCardsAction
//This should be renamed eventually to "DiscardAndDraw"
{
    public override void Execute(BattleManager battleManager)
    {
        if (caller is PlayerCombatant player)
        {
            var handManager = GameObject.FindFirstObjectByType<HandManager>();
            cardCount = (int)caller.EvaluateStatFormula("PSY/4");

            for (int i = handManager.cardsInHand.Count - 1; i >= 0; i--)
            {
                var cardGO = handManager.cardsInHand[i];
                var card = cardGO.GetComponent<CardDisplay>().card;
                player.DiscardCard(card);
                handManager.cardsInHand.RemoveAt(i);
                GameObject.Destroy(cardGO);
            }

            player.DrawCards(cardCount);
            handManager.InitializeHand(player.hand);
            GameManager.Instance.ShowMessage($"Drawing {cardCount} cards");
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
        base.Execute(battleManager);
    
    }
}

public class ChainOfPainAction : GameAction
{
    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        foreach(Combatant c in battleManager.currentTargets)
        {
            if(c is PlayerCombatant player && player.discard.Count > 1)
            {
                var card = player.discard[player.discard.Count-2];
                GameManager.Instance.ShowMessage($"{player.combatantName} uses {card.cardName} AGAIN!!!");
                foreach(GameAction a in card.effects)
                {
                    a.wildSwing = true;
                }
                battleManager.ExecuteCard(card,caller);
                 
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
        base.Execute(battleManager);
        if(battleManager.currentTargets[0] is PlayerCombatant p)
        {
            p.GainMP(caller.EvaluateStatFormula(mpAmount));
            GameManager.Instance.ShowMessage($"{caller.combatantName} winks at {p.combatantName}. You got this, girl! +{caller.EvaluateStatFormula(mpAmount)} MP");
        }
        else
        {
            float suckAmount = caller.EvaluateStatFormula(mpAmount);
            battleManager.currentTargets[0].GainMP(-caller.EvaluateStatFormula(mpAmount));
            caller.GainMP(suckAmount);
            GameManager.Instance.ShowMessage($"{caller.combatantName} winks at {battleManager.currentTargets[0].combatantName}, stealing their heart and {suckAmount} MP");

        }
        caller.PlayAnimation(animation);
    }
}

public class EnemyDrainAction : GameAction
{
    public int mpAmount;

    public override void Execute(BattleManager battleManager)
    {
        base.Execute(battleManager);
        if (caller is EnemyCombatant)
        {
            battleManager.SelectRandomTargets(caller, targetType);
        }
        var totalManaDrained = 0f;
        foreach(Combatant c in battleManager.currentTargets)        {
            if(c is PlayerCombatant p)
            {
                var manaStolen = Mathf.Min(p.mp, mpAmount);
                p.GainMP(-manaStolen);
                caller.GainMP(manaStolen);
                totalManaDrained += manaStolen;
            }
        }
        GameManager.Instance.ShowMessage($"{caller.combatantName} sucks {totalManaDrained} MP from your party!");
        caller.PlayAnimation(animation);
    }
}