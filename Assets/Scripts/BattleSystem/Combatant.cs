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
    float damagePerLevel = 2f;
    public float mp;
    public float maxMp;
    Vector3 startPosition;
    public bool alive = true;
    Vector3 targetPosition;
    public DamageType[] resistances;
    public DamageType[] weaknesses;

    //Misc
    public string combatantName;
    [HideInInspector] public float initiative = 0f;
    List<StatusEffect> statusEffects = new List<StatusEffect>();

    //Animation properties
    Transform model;
    RectTransform statusCanvas;
    Animator animator;
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        startPosition = transform.position;
        targetPosition = startPosition;
        statusCanvas = transform.Find("StatusCanvas").GetComponent<RectTransform>();
    }

    public float TakeDamage(Combatant caller, float baseDamage, DamageType damageType)
    {
        if(!alive) return 0f;
        baseDamage += level * damagePerLevel;
        baseDamage *= caller.EvaluateStatFormula("ATK") / (caller.EvaluateStatFormula("ATK")+EvaluateStatFormula("DEF")); //If attack and defense are equal, deal 1x damage. Higher attack deals more damage, higher defense reduces damage.
        var damageNumber = Instantiate(Resources.Load<GameObject>("DamageNumber"), transform.position, Quaternion.identity);
        var damageText = damageNumber.GetComponentInChildren<TMP_Text>();
        damageText.text = "";
        //1 in 20 chance to crit
        if(UnityEngine.Random.Range(1,21) == 1)
        {
            baseDamage *= 1.5f;
            damageText.text += "CRIT! ";
            damageText.color = Color.yellow;
        }
        if( resistances != null && System.Array.Exists(resistances, element => element == damageType))
        {
            baseDamage *= 0.5f; //Take half damage
            damageText.text += "TO THE ABS!";
            damageText.color = Color.cyan;
        }
        if( weaknesses != null && System.Array.Exists(weaknesses, element => element == damageType))
        {
            baseDamage *= 1.5f; //Take 1.5x damage
            damageText.text += "STRONG!";
            damageText.color = Color.yellow;
        }
        damageText.text += Mathf.RoundToInt(baseDamage).ToString();
        hp -= baseDamage;
        if(hp <= 0)
        {
            var skull = Instantiate(Resources.Load<GameObject>("Particles/Skull"), transform.position, Quaternion.identity);
            BattleManager.Instance.RemoveCombatant(this);
            GameManager.Instance.ShowMessage($"{combatantName} has been defeated!");
            alive = false;
            animator.Play("Death");
            OnDeath();
        }

        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            if (statusEffects[i].removeOnHit)
            {
                RemoveStatusEffect(statusEffects[i].name);
            }
        }
        
        return baseDamage;
    }

    public virtual void OnDeath(){}

    void Update()
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
        hp += amount;
        if (hp > maxHp) hp = maxHp;
         var damageNumber = Instantiate(Resources.Load<GameObject>("DamageNumber"), transform.position, Quaternion.identity);
        var damageText = damageNumber.GetComponentInChildren<TMP_Text>();
        damageText.text = $"+{Mathf.RoundToInt(amount)}";
        damageText.color = Color.green;
    }

    public void GainMP(float amount)
    {
        mp += amount;
        if (mp > maxMp) mp = maxMp;
         var damageNumber = Instantiate(Resources.Load<GameObject>("DamageNumber"), transform.position, Quaternion.identity);
        var damageText = damageNumber.GetComponentInChildren<TMP_Text>();
        damageText.text = $"+{Mathf.RoundToInt(amount)}";
        damageText.color = Color.magenta;
    }

    public float PlayAnimation(string animationName)
    {
        if(animator == null || string.IsNullOrEmpty(animationName)) return 1f;
        animator.Play(animationName);
        return animator.GetCurrentAnimatorStateInfo(0).length;
    }

    public void ApplyStatusEffect(StatusEffect effect)
    {
        if(effect == null) return;
        if(effect.name == "") return;
        if(effect.name == null) return;
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
        UpdateStatusVisuals();
    }

    public void RemoveStatusEffect(string effectName) //blank effectName removes all effects
    {
        if(effectName == "")
            statusEffects.Clear();
        else
            statusEffects.RemoveAll(e => e.name == effectName);
        UpdateStatusVisuals();
    }

    public void decreaseStatusEffects()
    {
        foreach(var effect in statusEffects)
        {
            if(effect.duration != -1)
                effect.duration--;
        }
        statusEffects.RemoveAll(e => e.duration <= 0 && e.duration != -1);
        UpdateStatusVisuals();
    }

    public virtual void StartTurn()
    {
        decreaseStatusEffects();
    }

    void UpdateStatusVisuals()
    {
        foreach(Transform child in statusCanvas) Destroy(child.gameObject);
        foreach(var effect in statusEffects)
        {
            var statusIcon = Instantiate(Resources.Load<GameObject>("StatusEffect"), statusCanvas);
            var iconImage = statusIcon.GetComponent<UnityEngine.UI.Image>();
            var sprite = Resources.Load<Sprite>($"Sprites/{effect.name}");
            iconImage.sprite = sprite;
            TMP_Text durationText = statusIcon.GetComponentInChildren<TMP_Text>();
            if(effect.duration != -1)
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
    public float EvaluateStatFormula(string statFormula)
    {
        statFormula = Regex.Replace(statFormula,@"\b[A-Z]+\b",match => GetStat(match.Value).ToString());
        var dataTable = new System.Data.DataTable();
        var result = dataTable.Compute(statFormula, "");
        return System.Convert.ToSingle(result);
    }


}
