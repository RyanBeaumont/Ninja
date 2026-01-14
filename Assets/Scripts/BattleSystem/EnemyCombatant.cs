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
    public int hits;
    public float mpCost;
    public float healthThreshold; //The enemy will only use this attack if its health is below this percentage (0-100)
}

public class EnemyCombatant : Combatant
{
    [SerializeField] List<EnemyAttackData> attackPatterns = new List<EnemyAttackData>();
    public List<LootDrop> lootDrops = new List<LootDrop>();
    public float xpReward = 10f;
    public float goldReward = 10f;
    public float attackSpeed = 0.25f;

    public void OnHit(string direction)
    {
        BattleManager.Instance.EnemyHit(direction);
    }

    public override void StartTurn()
    {
        base.StartTurn();
        mp += 10f;
        GameManager.Instance.ShowMessage($"{combatantName}'s turn!");
        Invoke("Attack", 2f);
    }


    void Attack()
    {
        if(HasStatusEffect("Stunned") != null)
        {
            GameManager.Instance.ShowMessage($"{combatantName} is stunned and cannot move!");
            return;
        }
        //select a random attack pattern that the enemy can afford and meets health threshold
        List<EnemyAttackData> availableAttacks = new List<EnemyAttackData>();
        foreach(var attack in attackPatterns)  
        {
            if(mp >= attack.mpCost && (hp / maxHp * 100f) <= attack.healthThreshold)
            {
                availableAttacks.Add(attack);
            }
        }
        if(availableAttacks.Count == 0)
        {
            //No available attacks, skip turn
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
        BattleManager.Instance.SelectRandomTargets(this, selectedAttack.targetType);

        //Split attack pattern by commas
        var attacks = selectedAttack.attackPattern.Split(',');
        foreach(var attack in attacks)
        {
            BattleManager.Instance.actionQueue.Add(new EnemyAttackAction()
            {
                caller = this,
                animation = attack.Trim(),
                //targets = BattleManager.Instance.currentTargets,
                statusEffect = selectedAttack.statusEffect,
                damage = selectedAttack.damage,
                damageType = selectedAttack.damageType,
                hits = selectedAttack.hits,
                timeScale = attackSpeed
            });
        }
    }
}
