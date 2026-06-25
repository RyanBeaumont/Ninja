using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System.Linq;


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
    public float mp;
    public float maxMp;
    [HideInInspector] public GameObject hpBar;
    [HideInInspector] public Vector3 startPosition;
    public bool alive = true;
    Vector3 targetPosition;
    public DamageType[] resistances;
    public DamageType[] weaknesses;
    public List<DamageType> discoveredResistances = new List<DamageType>();
    public List<DamageType> discoveredWeaknesses = new List<DamageType>();

    //Misc
    public string combatantName;
    [HideInInspector] public float initiative = 0f;
    public List<StatusEffect> statusEffects = new List<StatusEffect>();
    public Animator copyAnimator = null;

    //Animation properties
    Transform model;
    RectTransform statusCanvas;
    Animator animator;
    public virtual void Start()
    {
        CapsuleCollider collider = GetComponentInChildren<CapsuleCollider>();
        if (collider != null)
        {
            LayerMask groundMask = LayerMask.GetMask("Ground");
            RaycastHit hit;
            if (Physics.Raycast(collider.transform.position + Vector3.up * 1f, Vector3.down, out hit, 4f, groundMask))
            {
                float halfHeight = collider.height / 2f;
                Vector3 newPosition = hit.point + Vector3.up * halfHeight;
                collider.transform.position = newPosition;
                Debug.Log($"Adjusted {combatantName}'s position to {newPosition}");
            }
        }
        animator = GetComponentInChildren<Animator>();
        startPosition = transform.position;
        targetPosition = startPosition;
        defense = 1f; //Now a multiplier
        statusCanvas = transform.Find("StatusCanvas").GetComponent<RectTransform>();

        
    }

    public void EndTurn()
    {
        decreaseStatusEffects(statusUpdate: StatusUpdate.TurnEnd);
    }

    public virtual float TakeDamage(Combatant caller, float baseDamage, DamageType damageType)
    {
        if(!alive) return 0f;
        var strong = false;
        if(caller == null)return 0f;
        //Damage = BaseDamage × (Attack / AttackBaseline) × (K / (Defense + K))
        baseDamage = Mathf.Abs(baseDamage *EvaluateStatFormula("DEF")); 
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
        if(resistances != null && System.Array.Exists(resistances, element => element == damageType) && HasStatusEffect("Weak")==null)
        {
            baseDamage *= 0.5f; //Take half damage
            damageText.text += "Weak!";
            AudioManager.Instance.PlaySoundEffect("Anvil",Random.Range(0.9f,1.1f));
            damageText.color = color;
            if(!discoveredResistances.Contains(damageType)) discoveredResistances.Add(damageType);
        }
        if((weaknesses != null && System.Array.Exists(weaknesses, element => element == damageType)) || (HasStatusEffect("Weak") != null && damageType != DamageType.None)) //The ninja can't get infinite extra attacks
        {
            baseDamage *= 1.5f; //Take 1.5x damage
            damageText.text += "STRONG!";
            //spawn blood fx
            var blood = Instantiate(Resources.Load<GameObject>("Particles/Blood"),transform);
            if(!discoveredWeaknesses.Contains(damageType)&& System.Array.Exists(weaknesses, element => element == damageType)) discoveredWeaknesses.Add(damageType);
            Destroy(blood,0.5f);

            //Crit bonus
            if(caller is PlayerCombatant p)
            {
                var character = YourParty.instance.GetPartyMember(p.combatantName);
                if(character != null)
                {
                    if(character.mainClass == CardClass.Warrior)
                    {
                        p.DrawCards(1);
                        GameManager.Instance.ShowMessage($"<color=green>WARRIOR: {p.combatantName} draws a card on crit!</color>");
                    }
                    if(character.mainClass == CardClass.Ninja)
                    {
                        //Additional damage action
                        var action = new DamageAction()
                        {
                            caller = p,
                            damage = "5*LEVEL",
                            hits = 1,
                            damageType = DamageType.None,
                            animation = "SwordBackhand",
                            text = $"<color=green> NINJA: {p.combatantName} gets an extra attack on crit!</color>"
                        };
                        BattleManager.Instance.actionQueue.Insert(0,action);
                    }
                    if(character.mainClass == CardClass.Psychic)
                    {
                        var mpAmount = 5 + p.level;
                        p.GainMP(mpAmount);
                        GameManager.Instance.ShowMessage($"<color=green>PSYCHIC: {p.combatantName} gains {mpAmount} MP on crit!</color>");
                    }
                    if(character.mainClass == CardClass.Grappler)
                    {
                        var counterAmount = 5 + p.level;
                        p.ApplyStatusEffect(CardDatabase.Instance.getStatusEffect("Increased Counter"));
                        GameManager.Instance.ShowMessage($"<color=green>GRAPPLER: {p.combatantName}'s counter-hit gains +{counterAmount} damage</color>");
                    }
                    
                }
            }

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
            GameManager.Instance.ShowMessage($"<color=red>{combatantName} has been defeated!</color>");
            AudioManager.Instance.PlaySoundEffect("Explosion");
            if(animator != null) animator.Play("Launcher");
            alive = false;
            if(caller != null && caller != this){
            StatusEffect rocket = caller.HasStatusEffect("Rocket Fist");
            if(rocket != null && caller.alive)
            {
                caller.ApplyStatusEffect(CardDatabase.Instance.getStatusEffect("Rocket Fist Damage"));
                GameManager.Instance.ShowMessage($"Rocket Fist got a kill and is now up to +{caller.HasStatusEffect("Rocket Fist Damage").amount} damage");
            }
            }
            OnDeath();
        }

        var statusesToRemove = new List<string>();
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            if (statusEffects[i].removeOnHit && damageType != DamageType.Psychic)
            {
                statusesToRemove.Add(statusEffects[i].name);
            }
        }

        foreach (var effectName in statusesToRemove)
        {
            RemoveStatusEffect(effectName);
        }

        if (strong) //Check for exposed
        {
            if (HasStatusEffect("Exposed") != null)
            {
                ApplyStatusEffect(CardDatabase.Instance.getStatusEffect("Off-Balance"));
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
        if(HasStatusEffect("DeathBomb") != null)
        {
            GameManager.Instance.ShowMessage($"{combatantName} EXPLODES!");
            foreach(Combatant c in BattleManager.Instance.combatants.Where(c => c is EnemyCombatant)) c.TakeDamage(c,50,DamageType.Bludgeoning);
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
        var particleeffect = Instantiate(Resources.Load<GameObject>("Particles/Health"), transform);
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
        var particleeffect = Instantiate(Resources.Load<GameObject>("Particles/MP"), transform);
    }

    public float PlayAnimation(string animationName)
    {
        if(animator == null || string.IsNullOrEmpty(animationName) || alive == false) return 0.1f;
        animator.Play(animationName,0,0f);
        
        if(copyAnimator != null)
        {
            copyAnimator.Play(animationName,0,0f);
        }
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

        if(effect.particleEffect != null)
        {
            var fx = Instantiate(effect.particleEffect, transform);
            Destroy(fx, 2f);
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
        GameManager.Instance.ShowMessage($"<color=red>{combatantName} is affected by {effect.name}</color>");
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

    public void decreaseStatusEffects(StatusUpdate statusUpdate)
    {
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            var effect = statusEffects[i];
            if(effect.statusUpdate != statusUpdate) continue; //Only update the correct type
            if(effect.name == "Poisoned") // || effect.name == "Suplex
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

            BattleManager.Instance.actionQueue.Add(new SelfDamageAction()
            {
               caller = this,
               animation = "IdleDrunk",
               text =  $"{combatantName} takes {Mathf.Abs(6*poison.amount)} damage from poison",
               damage = $"{Mathf.Abs(6*poison.amount)}",
               damageType = DamageType.None,
            });
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
            GameManager.Instance.ShowMessage($"<color=red>{combatantName} is stunned and cannot move!</red>");
            success = false;
        }

        decreaseStatusEffects(statusUpdate: StatusUpdate.TurnStart);
        //decrease all players' status effects that have statusUpdate = CallerTurnStart and this is the caller
        foreach(Combatant c in BattleManager.Instance.combatants)
        {
            foreach(var effect in c.statusEffects)
            {
                if(effect.statusUpdate == StatusUpdate.CallerTurnStart && effect.caller == this)
                {
                    c.decreaseStatusEffects(StatusUpdate.CallerTurnStart);
                }
            }
        }
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
            var sprite = effect.sprite;
            if(sprite == null) sprite = Resources.Load<Sprite>($"Sprites/{effect.name}");
            iconImage.sprite = sprite;
            TMP_Text durationText = statusIcon.GetComponentInChildren<TMP_Text>();
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
            "LOW" => attack*0.05f,
            "MED" => attack*0.107f,
            "HIGH" => attack*0.214f,

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
