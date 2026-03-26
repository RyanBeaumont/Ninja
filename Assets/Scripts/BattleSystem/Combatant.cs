using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;


public class Combatant : MonoBehaviour
{
    //Combat stats
    public int level;
    public float maxHp;
    public float hp;
    public float attack;
    public float defense;
    public float speed;
    public float psychic;
    public bool surprise = false;
    float damagePerLevel = 2f;
    public float mp;
    public float maxMp;
    [HideInInspector] public Vector3 startPosition;
    public bool alive = true;
    Vector3 targetPosition;
    public DamageType[] resistances;
    public DamageType[] weaknesses;

    //Misc
    public string combatantName;
    [HideInInspector] public float initiative = 0f;
    public List<StatusEffect> statusEffects = new List<StatusEffect>();

    //Animation properties
    Transform model;
    RectTransform statusCanvas;
    Animator animator;
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        startPosition = transform.position;
        targetPosition = startPosition;
        defense = 1f; //Now a multiplier
        statusCanvas = transform.Find("StatusCanvas").GetComponent<RectTransform>();
    }

    public virtual float TakeDamage(Combatant caller, float baseDamage, DamageType damageType)
    {
        if(!alive) return 0f;
        var strong = false;
        if(caller != null){
            //Damage = BaseDamage × (Attack / AttackBaseline) × (K / (Defense + K))
            var multiplier = Mathf.Abs(caller.EvaluateStatFormula("ATK")/15 * EvaluateStatFormula("DEF"));
            print($"Atk: {caller.EvaluateStatFormula("ATK")} Base damage: {baseDamage} x Multiplier: {multiplier}");
            multiplier = Mathf.Clamp(multiplier,0.25f,6.0f);
            baseDamage = Mathf.Abs(baseDamage * multiplier); //If attack and defense are equal, deal 1x damage. Higher attack deals more damage, higher defense reduces damage.
        }
        
        
        var damageNumber = Instantiate(Resources.Load<GameObject>("DamageNumber"), transform.position, Quaternion.identity);
        var damageText = damageNumber.GetComponentInChildren<TMP_Text>();
        var color = Color.yellow;
        if(damageType == DamageType.Bludgeoning) color = Color.red;
        if(damageType == DamageType.Psychic) color = Color.magenta;
        damageText.text = "";
        //1 in 20 chance to crit
        /*if(UnityEngine.Random.Range(1,21) == 1)
        {
            baseDamage *= 1.5f;
            damageText.text += "CRIT! ";
            damageText.color = Color.yellow;
        }
        */
        if( resistances != null && System.Array.Exists(resistances, element => element == damageType))
        {
            baseDamage *= 0.5f; //Take half damage
            damageText.text += "Weak!";
            AudioManager.Instance.PlaySoundEffect("Anvil",Random.Range(0.9f,1.1f));
            damageText.color = color;
        }
        if( weaknesses != null && System.Array.Exists(weaknesses, element => element == damageType))
        {
            baseDamage *= 1.5f; //Take 1.5x damage
            damageText.text += "STRONG!";
            AudioManager.Instance.PlaySoundEffect("OrchestraHit",Random.Range(0.9f,1.1f));
            damageText.color = color;
            strong = true;
        }
        damageText.text += Mathf.RoundToInt(baseDamage).ToString();
        hp -= baseDamage;
        if(hp <= 0)
        {
            var skull = Instantiate(Resources.Load<GameObject>("Particles/Skull"), transform.position, Quaternion.identity);
            BattleManager.Instance.RemoveCombatant(this);
            GameManager.Instance.ShowMessage($"{combatantName} has been defeated!");
            AudioManager.Instance.PlaySoundEffect("Explosion");
            animator.Play("Launcher");
            alive = false;
            if(caller != null && caller != this){
            StatusEffect rocket = caller.HasStatusEffect("RocketFistActive");
            if(rocket != null && caller.alive)
            {
                caller.ApplyStatusEffect(new StatusEffect()
                {
                    name = "Rocket Fist",
                    amount = 10,
                    stat = "ATK"
                });
                GameManager.Instance.ShowMessage($"Rocket Fist got a kill and is now up to +{caller.HasStatusEffect("Rocket Fist").amount} damage");
            }
            }
            OnDeath();
        }

        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            if (statusEffects[i].removeOnHit && damageType != DamageType.Psychic)
            {
                RemoveStatusEffect(statusEffects[i].name);
            }
        }

        if (strong) //Check for exposed
        {
            if (HasStatusEffect("Exposed") != null)
            {
                ApplyStatusEffect(new StatusEffect
                {
                    name = "Off-Balance",
                        stat = "DEF",
                        amount = .25f,
                        duration = -1,
                        removeOnHit = true
                });
            }
        }

        if(HasStatusEffect("Lifesteal") != null && caller != null)
        {
            caller.Heal(baseDamage);
        }

        if(HasStatusEffect("Choking") != null) //Stop the choke
        {
            RemoveStatusEffect("Choking");
            foreach(Combatant c in BattleManager.Instance.combatants)
            {
                c.RemoveStatusEffect("Choked");
            }
        }
        
        return baseDamage;
    }

    public virtual void OnDeath()
    {
        if(this is PlayerCombatant){
            YourParty.instance.GetPartyMember(combatantName).alive = false;
        }
        else
        {
            Invoke("DieForReal", 3f);
        }
        if(HasStatusEffect("Choked") != null)
        {
            foreach(Combatant c in BattleManager.Instance.combatants) c.RemoveStatusEffect("Choking");
        }
    }

    void DieForReal()
    {
        SetTargetPosition(transform.position + Vector3.down * 2f);
        Destroy(gameObject,1f);
    }

    public virtual void Update()
    {
        //Smoothly move to target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
    }

    public void ReturnToStartPosition()
    {
        targetPosition = startPosition;
    }
    public void SetTargetPosition(Vector3 newPosition)
    {
        targetPosition = newPosition;
    }

    public void Heal(float amount)
    {
        if(alive){
        hp += amount;
        if (hp > maxHp) hp = maxHp;
         var damageNumber = Instantiate(Resources.Load<GameObject>("DamageNumber"), transform.position, Quaternion.identity);
        var damageText = damageNumber.GetComponentInChildren<TMP_Text>();
        damageText.text = $"+{Mathf.RoundToInt(amount)}";
        damageText.color = Color.green;
        }
        else
        {
            GameManager.Instance.ShowMessage("Can't Heal a Dead Hero");
        }
    }

    public void GainMP(float amount)
    {
        mp += amount;
        if (mp > maxMp) mp = maxMp;
        if(mp < 0) mp = 0;
         var damageNumber = Instantiate(Resources.Load<GameObject>("DamageNumber"), transform.position, Quaternion.identity);
        var damageText = damageNumber.GetComponentInChildren<TMP_Text>();
        damageText.text = $"{Mathf.RoundToInt(amount)}";
        damageText.color = Color.magenta;
    }

    public float PlayAnimation(string animationName)
    {
        if(animator == null || string.IsNullOrEmpty(animationName) || alive == false) return 0.1f;
        animator.Play(animationName,0,0f);
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }

     public float RestartAnimation()
    {
        if(animator == null) return 0.1f;
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        animator.Play(state.fullPathHash, 0, 0f);
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }


    public void ApplyStatusEffect(StatusEffect effect)
    {
        if(effect == null) return;
        if(effect.name == "") return;
        if(effect.name == null) return;

        if (HasStatusEffect("Linked") != null)
        {
            foreach(Combatant c in BattleManager.Instance.combatants)
            {
                if(c.CompareTag(gameObject.tag) && c != this && c.HasStatusEffect("Linked") == null) //Only chain to non-linked targets
                {
                    c.ApplyStatusEffect(effect);
                }
            }
        }
        //Check if effect is already applied
        var existingEffect = statusEffects.Find(e => e.name == effect.name);
        if(existingEffect != null)
        {
            //Refresh duration
            existingEffect.duration = effect.duration;
            existingEffect.amount += effect.amount;
        }
        else
        {
            statusEffects.Add(effect);
        }
        GameManager.Instance.ShowMessage($"{combatantName} is affected by {effect.name}");
        UpdateStatusVisuals();
    }

    public void RemoveStatusEffect(string effectName) //blank effectName removes all effects
    {
        if(effectName == "")
            statusEffects.RemoveAll(e => e.name != "Equipment");
        else
            statusEffects.RemoveAll(e => e.name == effectName);
        UpdateStatusVisuals();
    }

    public void decreaseStatusEffects()
    {
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            var effect = statusEffects[i];
            if(effect.name == "Poisoned")
            {
                effect.amount --;
                if(effect.amount <= 0) statusEffects.Remove(effect);
            }
            if(effect.duration != -1)
                effect.duration--;
        }
        statusEffects.RemoveAll(e => e.duration <= 0 && e.duration != -1);
        UpdateStatusVisuals();
    }

    public virtual bool StartTurn()
    {
        bool success = true;
        if(HasStatusEffect("Poisoned") != null)
        {
            var poison = HasStatusEffect("Poisoned");
            GameManager.Instance.ShowMessage($"{combatantName} takes {6*poison.amount} damage from poison");
            TakeDamage(null,6*poison.amount, DamageType.Psychic);
            if(hp <= 0){
                BattleManager.Instance.actionQueue.Add(new StunAction()
                {
                    caller = this,
                    animation = "Defeated",
                });
            return false;
            }
        }
         if(HasStatusEffect("Stunned") != null)
        {
            BattleManager.Instance.actionQueue.Add(new StunAction()
            {
                caller = this,
                animation = "Defeated",
            });
            GameManager.Instance.ShowMessage($"{combatantName} is stunned and cannot move!");
            success = false;
        }
        if(HasStatusEffect("Choked") != null)
        {
            BattleManager.Instance.actionQueue.Add(new SelfDamageAction()
            {
                caller = this,
                damage = "30",
                animation = "Defeated",
            });
            GameManager.Instance.ShowMessage($"{combatantName} is being choked!");
            success = false;
        }
        if(HasStatusEffect("Choking") != null)
        {
            BattleManager.Instance.actionQueue.Add(new StunAction()
            {
                caller = this,
                animation = "ArmsCrossed",
            });
            GameManager.Instance.ShowMessage($"{combatantName} has the enemy locked in a chokehold");
            success = false;
        }

        decreaseStatusEffects();
        return success;
    }

    void UpdateStatusVisuals()
    {
        foreach(Transform child in statusCanvas) Destroy(child.gameObject);
        foreach(var effect in statusEffects)
        {
            if(effect.name == "Equipment") continue;
            var statusIcon = Instantiate(Resources.Load<GameObject>("StatusEffect"), statusCanvas);
            var iconImage = statusIcon.GetComponent<UnityEngine.UI.Image>();
            var sprite = Resources.Load<Sprite>($"Sprites/{effect.name}");
            if(sprite == null) sprite = Resources.Load<Sprite>($"Items/{effect.name}");
            iconImage.sprite = sprite;
            TMP_Text durationText = statusIcon.GetComponentInChildren<TMP_Text>();
            if(effect.amount > 1)
                durationText.text = effect.amount.ToString();
            else if(effect.duration != -1)
                durationText.text = effect.duration.ToString();
            else
                durationText.text = "";
        }
    }

    float GetStat(string statName)
    {
        float baseValue = statName switch
        {
            "ATK" => attack,
            "DEF" => defense,
            "PSY" => psychic,
            "SPD" => speed,
            "MAXHP" => maxHp,
            "HP" => hp,
            "LEVEL" => level,
            "MP" => mp,
            "MAXMP" => maxMp,
            _ => throw new System.Exception($"Unknown stat: {statName}")
        };

        //Apply status effect modifiers
        foreach(var effect in statusEffects)
        {
            if(effect.stat == statName)
            {
                if(effect.additive)
                    baseValue += effect.amount;
                else
                    baseValue *= effect.amount;
            }
        }

        return baseValue;
    }

    public StatusEffect HasStatusEffect(string effectName)
    {
        return statusEffects.Find(e => e.name == effectName);
    }

    public float EvaluateStatFormula(string statFormula)
    {
        statFormula = Regex.Replace(statFormula,@"\b[A-Z]+\b",match => GetStat(match.Value).ToString());
        var dataTable = new System.Data.DataTable();
        var result = dataTable.Compute(statFormula, "");
        return System.Convert.ToSingle(result);
    }


}
