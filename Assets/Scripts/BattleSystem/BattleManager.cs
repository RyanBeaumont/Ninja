using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine.UI;

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
    }
}

public class ChooseTargetsAction : GameAction
{
    public string prompt;
    public GameAction gameAction; //action to perform after targeting

    public override void Execute(BattleManager battleManager)
    {
        Targeter targeter = Object.Instantiate(Resources.Load<GameObject>("Targeter")).GetComponent<Targeter>();
        targeter.Initialize(targetType, prompt, gameAction);
        battleManager.waitingForInput = true;
    }
}

public class EnemyAttackAction : GameAction
{
    public string damage;
    public int hits;
    public DamageType damageType;
    public StatusEffect statusEffect = null;
    public float timeScale = 0.25f;

    public override void Execute(BattleManager battleManager)
    {
        AudioManager.Instance.PlaySoundEffect("s_dbz_jump",Random.Range(0.8f,1.2f));
        battleManager.waitingForInput = true; //wait for animation input
        battleManager.hitsRemaining = hits;
        battleManager.pendingDamage = caller.EvaluateStatFormula(damage);
        Time.timeScale = timeScale; //slow down time for dramatic effect
        battleManager.pendingDamageType = damageType;
        if(statusEffect != null && statusEffect.name != "")
            battleManager.pendingStatusEffect = statusEffect;
        battleManager.canDodge = true;
        caller.PlayAnimation(animation);
    }
}

public class DamageAction : GameAction
{
    public string damage;
    public int hits;
    public DamageType damageType;
    public StatusEffect statusEffect = null;

    public override void Execute(BattleManager battleManager)
    {
        AudioManager.Instance.PlaySoundEffect("s_dbz_jump",Random.Range(0.8f,1.2f));
        battleManager.waitingForInput = true; //wait for animation input
        battleManager.hitsRemaining = hits;
        battleManager.pendingDamage = caller.EvaluateStatFormula(damage);
        battleManager.pendingDamageType = damageType;
        battleManager.pendingStatusEffect = statusEffect;
        caller.PlayAnimation(animation);
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

public class StatusEffectAction : GameAction
{
    public StatusEffect statusEffect;

    public override void Execute(BattleManager battleManager)
    {
        battleManager.clock = caller.PlayAnimation(animation);
        foreach(var t in battleManager.currentTargets)
        {
            t.ApplyStatusEffect(statusEffect);
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

public class BattleManager : MonoBehaviour
{
    public List<Combatant> combatants = new List<Combatant>();
    public List<GameAction> actionQueue = new List<GameAction>();
    public float clock = 0f;
    public bool waitingForInput = false;
    float waitTime = 1f;
    public float pendingDamage = 0f;
    public DamageType pendingDamageType;
    public StatusEffect pendingStatusEffect;
    public PlayerCombatant activePlayer = null;
    public Combatant activeCombatant = null;
    public RectTransform TurnOrderUI;
    public RectTransform playerStats;
    public Transform itemContainer;
    public Transform buttonContainer;
    public int attacksRemaining = 1;
    public int hitsRemaining = 0;
    public bool canDodge = false;
    public bool perfectDodge = false;
    bool executingActions = false;
    bool canWin = true;
    GameObject cameraRig;
    Animator cameraAnimator;
    HandManager handManager;
    public List<Combatant> currentTargets = new List<Combatant>();
    public static BattleManager Instance;

    //Dodge system
    float dodgeWindow = 0.25f;
    public float dodgeInputWindow = 0.25f;
    string dodgeInput = "";
    public float dodgeCooldown = 1f;
    public bool lifestrike = false;
    float goldReward;
    float xpReward;
    GameObject player;
    List<LootDrop> lootRewards = new List<LootDrop>();

    void Awake()
    {
        cameraRig = Instantiate(Resources.Load<GameObject>("CameraRig"));
        cameraRig.GetComponentInChildren<CinemachineCamera>().Priority = 8;
        cameraAnimator = cameraRig.GetComponentInChildren<Animator>();
        //singeton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateTurnOrderUI();
        Invoke("NextTurn", 0.1f);
        handManager = FindFirstObjectByType<HandManager>();
        handManager.SetHandActive(false);
        foreach(var c in combatants)
        {
            if(c is EnemyCombatant enemy)
            {
                goldReward += enemy.goldReward;
                xpReward += enemy.xpReward;
                lootRewards.AddRange(enemy.lootDrops);
            }
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        itemContainer.gameObject.SetActive(false);
    }


    public void StartBattle()
    {
    }

    public void SelectTargets(List<Combatant> targets)
    {
        currentTargets = targets;
    }

    public void EndAction()
    {
        waitingForInput = false;
        clock = waitTime;
        canDodge = false;
        Time.timeScale = 1f; //reset time scale
    }

    public void AddCombatant(Combatant combatant)
    {
        combatants.Add(combatant);
        UpdateTurnOrderUI();
    }

    public void RemoveCombatant(Combatant combatant)
    {
        combatants.Remove(combatant);
        UpdateTurnOrderUI();
    }


    public void ExecuteCard(Card card, Combatant caller)
    {
        print("Executing card: " + card.cardName);
        foreach(var action in card.effects)
        {
            action.caller = caller;
            if(action.targetType == TargetType.AllEnemies){SelectTargets(GameObject.FindGameObjectsWithTag("Enemy").ToList().Select(go => go.GetComponent<Combatant>()).ToList());actionQueue.Add(action);} 
            else if(action.targetType == TargetType.AllAllies){SelectTargets(GameObject.FindGameObjectsWithTag("PlayerCombatant").ToList().Select(go => go.GetComponent<Combatant>()).ToList());actionQueue.Add(action);}
            else if(action.targetType == TargetType.Self){SelectTargets(new List<Combatant>(){caller});actionQueue.Add(action);}
            else if(action.targetType == TargetType.None){SelectTargets(new List<Combatant>());actionQueue.Add(action);}
            else{
                var targetAction = new ChooseTargetsAction()
                {
                    targetType = action.targetType,
                    prompt = "Choose your target",
                    gameAction = action,
                    caller = caller
                };
                GameManager.Instance.ShowMessage("Choose your target for " + card.cardName);
                actionQueue.Add(targetAction);
            }
            
        }
    }

    public void SelectRandomTargets(Combatant caller, TargetType targetType)
    {
        List<Combatant> possibleTargets = new List<Combatant>();
        if(targetType == TargetType.SingleEnemy || targetType == TargetType.AllEnemies)
        {
            //players in activeCombatants
            possibleTargets = combatants.Where(c => c.tag == "PlayerCombatant" && c.alive).ToList();
            if(targetType == TargetType.AllEnemies){
                currentTargets = new List<Combatant>(possibleTargets);
                Transform spawnPoint = GameObject.Find("BattleSetup/PlayerSpawn").transform;
                SetPose(spawnPoint.transform, "", CameraAngle.wideBehind, "");
            }
            else
            {
                
                var target = possibleTargets[Random.Range(0, possibleTargets.Count)];
                caller.SetTargetPosition(target.transform.position + target.transform.forward * 2f);
                currentTargets = new List<Combatant>() { target };
                SetPose(target.transform, "", CameraAngle.behind, "");
            }
        }
        else if(targetType == TargetType.SingleAlly || targetType == TargetType.AllAllies)
        {
            possibleTargets = combatants.Where(c => c.tag == "Enemy" && c.alive).ToList();
            if(targetType == TargetType.AllAllies){
                currentTargets = new List<Combatant>(possibleTargets);
                Transform spawnPoint = GameObject.Find("BattleSetup/EnemySpawn").transform;
                SetPose(spawnPoint.transform, "", CameraAngle.wideBehind, "");
            }
            else
            {
                var target = possibleTargets[Random.Range(0, possibleTargets.Count)];
                currentTargets = new List<Combatant>() { target };
                SetPose(target.transform, "", CameraAngle.standard, "");
            }
        }
        else if(targetType == TargetType.Self)
        {
            currentTargets = new List<Combatant>() { caller };
            SetPose(caller.transform, "", CameraAngle.lowAngle, "");
        }
        else if(targetType == TargetType.None)
        {
            currentTargets = new List<Combatant>();
        }
        else if(targetType == TargetType.Any)
        {
            possibleTargets = combatants;
            var target = possibleTargets[Random.Range(0, possibleTargets.Count)];
            currentTargets = new List<Combatant>() { target };
            SetPose(target.transform, "", CameraAngle.standard, "");
        }

        
    }

    void Win()
    {
        foreach(var c in combatants)
        {
            if(c is PlayerCombatant pc)
            {
                var partyMember = YourParty.instance.GetPartyMember(pc.combatantName);
                partyMember.hpPercentage = pc.hp / pc.maxHp;
            }
        }
        var dialog = YourParty.instance.LevelUp((int)xpReward,(int)goldReward);
        foreach(var loot in lootRewards)
        {
            float roll = Random.Range(0f, 100f);
            if(roll <= loot.dropChance)
            {
                GameManager.Instance.AddInventoryItem(loot.itemID,1);
                dialog.Add(new Dialog()
                {
                    name = "",
                    text = $"{loot.itemID} found!",
                    cameraAngle = CameraAngle.closeup,
                    face = "Happy",
                    pose = "ArmsCrossed",
                    character = null
                });
            }
        }

        DialogBox d = FindFirstObjectByType<DialogBox>();
        d.StartDialog(dialog);
        d.OnDialogFinished += OnDialogFinished;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    
    }

    void Lose()
    {
        DialogBox d = FindFirstObjectByType<DialogBox>();
        d.StartDialog(new List<Dialog>()
        {
            new Dialog()
            {
                name = "",
                text = "Your party has been defeated...",
                cameraAngle = CameraAngle.highAngle,
                face = "Mad",
                pose = "Defeated",
                character = null
            }
        });
        d.OnDialogFinished += OnLoseDialogFinished;
    }

    void OnLoseDialogFinished()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        YourParty.instance.LoadLastSave();
    }

    void OnDialogFinished()
    {
        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
        //Destroy(cameraRig);
        Destroy(gameObject);
        //unsubscribe
        var d = FindFirstObjectByType<DialogBox>();
        d.OnDialogFinished -= OnDialogFinished;
    }

    void Update()
    {
        if(canWin == false) return;

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(itemContainer.gameObject.activeInHierarchy)
            {
                itemContainer.gameObject.SetActive(false);
                buttonContainer.gameObject.SetActive(true);
                activePlayer.BonusTurn();
            }
        }
        //Check for win
        var enemies = combatants.Where(c => c.tag == "Enemy" && c.alive).ToList();
        
        if (combatants.Count > 0 && enemies.Count == 0 && canWin)
        {
            Invoke("Win", 2f);
            canWin = false;
        }

        var players = combatants.Where(c => c.tag == "PlayerCombatant" && c.alive).ToList();

        if(combatants.Count > 0 && players.Count == 0)
        {
            GameManager.Instance.ShowMessage("Defeat...");
            Invoke("Lose", 2f);
            canWin = false;
        }

        if(dodgeCooldown > 0f) dodgeCooldown -= Time.unscaledDeltaTime;
        if(dodgeWindow > 0f) dodgeWindow -= Time.unscaledDeltaTime; else {dodgeInput = "";print("Dodge window ended");}

        //Dodge system
        if(canDodge)
        {
            if(dodgeCooldown <= 0f)
            {
                if (Input.GetKeyDown(KeyCode.A)){ dodgeInput = "Left"; }
                if (Input.GetKeyDown(KeyCode.D)) dodgeInput = "Right";
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) dodgeInput = "Jump";
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.LeftShift)) dodgeInput = "Duck";
                if(Input.GetKeyDown(KeyCode.Mouse1)) dodgeInput = "Block";

                if(dodgeInput != "")
                {
                    if(currentTargets.Count == 1)
                    {
                        if(dodgeInput == "Left")
                        {
                            cameraAnimator.Play("Camera_DodgeLeft");
                        }
                        else if(dodgeInput == "Right")
                        {
                            cameraAnimator.Play("Camera_DodgeRight");
                        }
                        else if(dodgeInput == "Jump")
                        {
                            cameraAnimator.Play("Camera_Jump");
                        }
                        else if(dodgeInput == "Duck")
                        {
                            cameraAnimator.Play("Camera_Duck");
                        }
                    }
                    AudioManager.Instance.PlaySoundEffect("Whoosh",Random.Range(0.8f,1.2f));
                    dodgeWindow = dodgeInputWindow;
                    dodgeCooldown = 0.5f;
                    foreach(var t in currentTargets) t.PlayAnimation(dodgeInput);
                }
            }
        }

        // Countdown when clock is active (> 0)
        if (!waitingForInput)
        {
            if(clock > 0f) clock -= Time.deltaTime;
            else if (actionQueue.Count > 0)
            {
                buttonContainer.gameObject.SetActive(false);
                executingActions = true;
                handManager.SetHandActive(false);
                var action = actionQueue[0];
                actionQueue.RemoveAt(0);
                clock = waitTime;
                waitingForInput = false;
                attacksRemaining += action.bonusActions;
                action.Execute(this);
            }
            else
            {
                if (executingActions) //just finished executing actions
                {
                    executingActions = false;
                    attacksRemaining --;
                    if(attacksRemaining <= 0)
                    {
                        NextTurn();
                        attacksRemaining = 1;
                    }else{
                        if(activePlayer != null) activePlayer.BonusTurn();
                    }
                }
            }
        }
    
    }

   void UpdateTurnOrderUI()
    {
        foreach (Transform child in TurnOrderUI)
            Destroy(child.gameObject);

        var upcoming = SimulateNextTurns(6);

        foreach (var combatant in upcoming)
        {
            var icon = Instantiate(
                Resources.Load<GameObject>("TurnIcon"),
                TurnOrderUI
            );

            icon.GetComponentInChildren<TMP_Text>().text =
                combatant.name.Substring(0, 1);
        }
    }

    List<Combatant> SimulateNextTurns(int count)
{
    const float TURN_THRESHOLD = 100f;
    const int MAX_ITERATIONS = 10000;

    var sim = new List<(Combatant c, float initiative)>();
    foreach (var c in combatants)
        sim.Add((c, c.initiative));

    var result = new List<Combatant>();

    for (int i = 0; i < count; i++)
    {
        int iterations = 0;

        while (iterations++ < MAX_ITERATIONS)
        {
            for (int j = 0; j < sim.Count; j++)
                sim[j] = (sim[j].c, sim[j].initiative + Mathf.Max(0.01f, sim[j].c.speed));

            int bestIndex = -1;
            float highest = 0f;

            for (int j = 0; j < sim.Count; j++)
            {
                if (sim[j].initiative >= TURN_THRESHOLD &&
                    sim[j].initiative > highest)
                {
                    highest = sim[j].initiative;
                    bestIndex = j;
                }
            }

            if (bestIndex != -1)
            {
                var chosen = sim[bestIndex];
                chosen.initiative -= TURN_THRESHOLD;
                sim[bestIndex] = chosen;

                result.Add(chosen.c);
                break;
            }
        }

        if (iterations >= MAX_ITERATIONS)
        {
            //Debug.LogError("Turn simulation failed — check combatant speeds.");
            break;
        }
    }

    return result;
}

    public void NextTurn()
    {
        itemContainer.gameObject.SetActive(false);
        buttonContainer.gameObject.SetActive(false);
        foreach(Combatant combatant in combatants)
        {
            combatant.PlayAnimation("Fighting");
            combatant.ReturnToStartPosition();
        }
        perfectDodge = true;
        playerStats.gameObject.SetActive(false);
        
        if (combatants.Count == 0) return;
        Combatant current = SimulateNextTurns(1).FirstOrDefault();
        if (current == null) return;
        // Consume initiative
        current.initiative -= 100f;
        current.StartTurn();
        activeCombatant = current;
        if(current is PlayerCombatant)
        {
            activePlayer = (PlayerCombatant)current;
            SetPose(current.transform, "Idle", CameraAngle.behind, "Mad");
            buttonContainer.gameObject.SetActive(true);
        }
        else
        {
            SetPose(current.transform, "Idle", CameraAngle.closeup, "Mad");
            activePlayer = null;
        }
        
        UpdateTurnOrderUI();
    }

    public void SkipTurn()
    {
        handManager.SetHandActive(false);
        NextTurn();
    }

    public void UseCoke()
    {
        var action = new HealAction()
        {
            caller = activePlayer,
            targetType = TargetType.SingleAlly,
            animation = "Drink",
            healAmount = "50"
        };
        var targetAction = new ChooseTargetsAction()
        {
            targetType = action.targetType,
            prompt = "Who Will Drink the Coke?",
            gameAction = action,
            caller = activePlayer
        };
        GameManager.Instance.ShowMessage("Who Will Drink the Coke?");
        actionQueue.Add(targetAction);
        itemContainer.gameObject.SetActive(false);
        buttonContainer.gameObject.SetActive(false);
    }

    public void ShowItemDisplay()
    {
        itemContainer.gameObject.SetActive(true);
        var inventory = itemContainer.GetComponentInChildren<Inventory>();
        inventory.UpdateInventoryImages(GameManager.Instance.inventory);
        var activeCardDisplays = FindObjectsByType<CardDisplay>(FindObjectsSortMode.None);
        buttonContainer.gameObject.SetActive(false);
        handManager.SetHandActive(false);
    }

    public void PlayerHit()
    {
        hitsRemaining --;
        //damage all targets
        foreach(var t in currentTargets)
        {
            t.TakeDamage(activeCombatant,(int)pendingDamage, pendingDamageType);
            var effect = Instantiate(Resources.Load<GameObject>("Particles/Hit"), t.transform);
            if(hitsRemaining == 0) t.PlayAnimation("Knockdown");
            else t.PlayAnimation("Stunned");
            if(pendingDamageType == DamageType.Slashing)
                AudioManager.Instance.PlaySoundEffect("HitSlash",Random.Range(0.8f,1.2f));
            if(pendingDamageType == DamageType.Bludgeoning)
                AudioManager.Instance.PlaySoundEffect("s_punch",Random.Range(0.8f,1.2f));
            if(pendingDamageType == DamageType.Psychic)
                AudioManager.Instance.PlaySoundEffect("Crackle",Random.Range(0.8f,1.2f));
            var d = t.TakeDamage(activeCombatant,(int)pendingDamage, pendingDamageType);
            if(lifestrike){lifestrike = false; activeCombatant.Heal(d);}
            if(activePlayer != null) activePlayer.tp += (int)(d / (activePlayer.level*0.75f)); //Gain TERROR points based on damage dealt
        }
        if(hitsRemaining <= 0)
        {
            if(pendingStatusEffect != null)
            {
                foreach(var t in currentTargets)
                {
                    t.ApplyStatusEffect(pendingStatusEffect);
                }
                pendingStatusEffect = null;
            }
            EndAction();
        }
    }
    public void EnemyHit(string direction)
    {
        
        if(direction == dodgeInput || (direction == "Dodge" && dodgeInput == "Left") || (direction == "Dodge" && dodgeInput == "Right"))
        {
            //successful dodge
            Debug.Log("Dodge successful!");
            dodgeCooldown = 0;
            dodgeInput = "";
            //GameManager.Instance.SoundEffect("Parry");
            foreach(var t in currentTargets)
            {
                var effect = Instantiate(Resources.Load<GameObject>("Particles/Block"), t.transform);
            }
        }
        else if(dodgeInput == "Block")
        {
            pendingDamage *= 0.5f; //Take half damage on block
            dodgeCooldown = 0;
            dodgeInput = "";
            perfectDodge = false;
            AudioManager.Instance.PlaySoundEffect("SwordClang",Random.Range(0.8f,1.2f));
            foreach(var t in currentTargets)
            {
                var effect = Instantiate(Resources.Load<GameObject>("Particles/Block"), t.transform);
                t.PlayAnimation("BlockSuccess");
                t.TakeDamage(activeCombatant,(int)pendingDamage, pendingDamageType);
            }
        }
        else{
            perfectDodge = false;
            foreach(var t in currentTargets)
            {
                var effect = Instantiate(Resources.Load<GameObject>("Particles/Hit"), t.transform);
                t.TakeDamage(activeCombatant,(int)pendingDamage, pendingDamageType);
                if(hitsRemaining == 0) t.PlayAnimation("Knockdown");
                else t.PlayAnimation("Stunned");
                if(pendingDamageType == DamageType.Slashing)
                AudioManager.Instance.PlaySoundEffect("HitSlash",Random.Range(0.8f,1.2f));
                if(pendingDamageType == DamageType.Bludgeoning)
                    AudioManager.Instance.PlaySoundEffect("s_punch",Random.Range(0.8f,1.2f));
                if(pendingDamageType == DamageType.Psychic)
                    AudioManager.Instance.PlaySoundEffect("Crackle",Random.Range(0.8f,1.2f));
            }
        }
        hitsRemaining --;
        if(hitsRemaining <= 0)
        {
            if(perfectDodge && actionQueue.Count == 0) //you dodged perfectly and there are no more actions queued
            {
                AudioManager.Instance.PlaySoundEffect("SwordClang",Random.Range(0.8f,1.2f));
                GameManager.Instance.ShowMessage($"Counter!");
                foreach(var t in currentTargets)
                {
                    if(t is PlayerCombatant){
                        actionQueue.Insert(0, new DamageAction()
                        {
                            caller = t,
                            animation = "SwordWhirlwind",
                            damage = "DEF",
                            damageType = DamageType.Psychic,
                            hits = 1
                        });
                    }
                    SelectTargets(new List<Combatant>() { activeCombatant });
                }
                
            }
            if(pendingStatusEffect != null && pendingStatusEffect.name != "")
            {
                pendingStatusEffect = null;
                if(!perfectDodge)
                    foreach(var t in currentTargets)
                    {
                        t.ApplyStatusEffect(pendingStatusEffect);
                    }
            }
            EndAction();
        }
    }

    void SetPose(Transform target, string pose, CameraAngle cameraAngle, string face)
    {
        if(target == null) return;
        if(cameraAnimator == null || cameraRig == null) return;
        Animator anim = target.GetComponentInChildren<Animator>();
        if(anim != null && pose != ""){
            if(!anim.GetCurrentAnimatorStateInfo(0).IsName(pose)){
                anim.CrossFade(pose, 0.05f);
                var pulse = target.GetComponentInChildren<PulseToTheBeat>();
                if(pulse != null) pulse.Pulse();
            }
           
        }
        if(face != "")
        {
            FaceChanger f = target.GetComponentInChildren<FaceChanger>();
            if(f != null)
            {
                f.ChangeFace(face);
            }
        }
        cameraRig.transform.parent = target;
        cameraRig.transform.localRotation = Quaternion.identity;
        cameraRig.transform.localPosition = new Vector3(0f,0f,0f);
        if(target.tag == "Player")
        {
            cameraRig.transform.localPosition += new Vector3(0f,-0.4f,0f);
        }
        if(cameraAngle == CameraAngle.standard) cameraAnimator.Play("Camera_OTS_Left");
        else if(cameraAngle == CameraAngle.closeup) cameraAnimator.Play("Camera_Closeup");
        else if(cameraAngle == CameraAngle.behind) cameraAnimator.Play("Camera_Behind");
        else if(cameraAngle == CameraAngle.lowAngle) cameraAnimator.Play("Camera_LowAngle");
        else if(cameraAngle == CameraAngle.highAngle) cameraAnimator.Play("Camera_HighAngle");
        else if(cameraAngle == CameraAngle.zoom) cameraAnimator.Play("Camera_Zoom");
        else if(cameraAngle == CameraAngle.tilt) cameraAnimator.Play("Camera_Tilt");
        else if(cameraAngle == CameraAngle.wideBehind) cameraAnimator.Play("Camera_WideBehind");
        else if(cameraAngle == CameraAngle.dodgeLeft) cameraAnimator.Play("Camera_DodgeLeft");
        else if(cameraAngle == CameraAngle.dodgeRight) cameraAnimator.Play("Camera_DodgeRight");
        else if(cameraAngle == CameraAngle.jump) cameraAnimator.Play("Camera_Jump");
        else if(cameraAngle == CameraAngle.duck) cameraAnimator.Play("Camera_Duck");
    }
}
