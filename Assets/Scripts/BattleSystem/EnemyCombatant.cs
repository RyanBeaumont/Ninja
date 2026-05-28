using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class LootDrop
{
    public string itemID;
    public float dropChance; //Percentage chance (0-100) of this item dropping
}

[Serializable]
public class EnemyAttackData
{
    public string attackPattern;
    public string attackName;
    public string damage;
    public DamageType damageType;
    public TargetType targetType;
    public StatusEffect statusEffect;
    public int hits = 1;
    public float mpCost = 0;
    public float healthThreshold = 100; //The enemy will only use this attack if its health is below this percentage (0-100)
    public string altFunction = ""; //If this is specified, the attack will instead call this function
}

public class EnemyCombatant : Combatant
{
    [SerializeField] List<EnemyAttackData> attackPatterns = new List<EnemyAttackData>();
    public List<LootDrop> lootDrops = new List<LootDrop>();
    public float xpReward = 10f;
    public float goldReward = 10f;
    public float attackSpeed = 0.25f;
    public Sprite portrait;
    public bool boss = false;
    [HideInInspector] public EnemyAttackData tempAttackData;

    public virtual void OnHit(string direction)
    {
        BattleManager.Instance.EnemyHit(direction);
    }

    public virtual void OnHitSuccess()
    {
        
    }

    public override bool StartTurn()
    {
        if(base.StartTurn()){
            mp += psychic;
            if(mp > maxMp) mp = maxMp;
            GameManager.Instance.ShowMessage($"{combatantName}'s turn!");
            Invoke("DefaultAttack", 2f);
        }
        return true;
    }

    public virtual void DefaultAttack()
    {
        Attack(null);
    }

    public void Attack(List<EnemyAttackData> attacksToUse = null, Combatant specialTarget = null)
    {
        var speed = attackSpeed;
        
        if(HasStatusEffect("Speed Up") != null)
        {
            speed += HasStatusEffect("Speed Up").amount;
            RemoveStatusEffect("Speed Up");
            GameManager.Instance.ShowMessage($"Enemy is now at {speed}x speed");
        }
        
        //select a random attack pattern that the enemy can afford and meets health threshold
        List<EnemyAttackData> tempData = attackPatterns;
        if(attacksToUse != null) tempData = attacksToUse;
        List<EnemyAttackData> availableAttacks = new List<EnemyAttackData>();
        foreach(var attack in tempData)  
        {
            if(mp >= attack.mpCost && (hp / maxHp * 100f) <= attack.healthThreshold)
            {
                availableAttacks.Add(attack);
            }
        }
        if(availableAttacks.Count == 0)
        {
            //No available attacks, skip turn
            Debug.Log("No available attacks");
            return;
        }
        // choose the highest MP-cost attack that is affordable; if multiple share the same
        // highest cost, pick one of them at random
        float maxCost = 0f;
        foreach (var a in availableAttacks)
        {
            if (a.mpCost > maxCost)
                maxCost = a.mpCost;
        }
        List<EnemyAttackData> highestAttacks = new List<EnemyAttackData>();
        foreach (var a in availableAttacks)
        {
            if (Mathf.Approximately(a.mpCost, maxCost))
                highestAttacks.Add(a);
        }
        var selectedAttack = highestAttacks[UnityEngine.Random.Range(0, highestAttacks.Count)];
        mp -= selectedAttack.mpCost;


        GameManager.Instance.ShowMessage($"{combatantName} uses {selectedAttack.attackName}!");
        //if(attacksToUse == null) //Only randomize targets if not countering
            

        //Split attack pattern by commas

        if(selectedAttack.altFunction != "")
        {
            tempAttackData = selectedAttack;
            Invoke(selectedAttack.altFunction,0f);
        }
        else
        {
            if(string.IsNullOrEmpty(selectedAttack.attackPattern))
            {
                Debug.LogWarning($"{combatantName} attack '{selectedAttack.attackName}' has no attackPattern.");
                return;
            }

            var effect = selectedAttack.statusEffect != null
                ? CardDatabase.Instance.getStatusEffect(selectedAttack.statusEffect.name, selectedAttack.statusEffect.amount, selectedAttack.statusEffect.duration)
                : null;

            var attacks = selectedAttack.attackPattern.Split(',');
            foreach(var attack in attacks)
            {
                BattleManager.Instance.actionQueue.Add(new EnemyAttackAction()
                {
                    caller = this,
                    animation = attack.Trim(),
                    //targets = BattleManager.Instance.currentTargets,
                    statusEffect = effect,
                    damage = selectedAttack.damage,
                    targetType = selectedAttack.targetType,
                    damageType = selectedAttack.damageType,
                    specialTarget = specialTarget,
                    hits = selectedAttack.hits,
                    //Loop animation only if named ThrowKnife or ThrowKnifeFast
                    loopAnimation = attack.Trim() == "ThrowKnife" || attack.Trim() == "ThrowKnifeFast",
                    timeScale = speed
                });
            }
        }
    }


    public void BuffAlly()
    {
        BattleManager.Instance.actionQueue.Add(new StatusEffectAction()
        {
            caller = this,
            animation = tempAttackData.attackPattern,
            targetType = tempAttackData.targetType,
            statusEffect = CardDatabase.Instance.getStatusEffect(tempAttackData.statusEffect.name,tempAttackData.statusEffect.amount,tempAttackData.statusEffect.duration),
        });
    }

    
}