using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;



public class BattleManager : MonoBehaviour
{
    public Action onWin;
    public List<Combatant> combatants = new List<Combatant>();
    public List<GameAction> actionQueue = new List<GameAction>();
    public bool loopAnimation = false;
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
    public int hitCounter = 0;
    public int discardPower = 0;
    public FloatValue gameDifficulty;
    bool gainTP = true;
    bool executingActions = false;
    bool canWin = true;
    public bool waitingForQuickTime = false;
    float pitch = 1f;
    [HideInInspector] public string pattern = "";
    GameObject cameraRig;
    Animator cameraAnimator;
    HandManager handManager;
    public List<Combatant> currentTargets = new List<Combatant>();
    public static BattleManager Instance;
    public Transform quickTimeEvent;
    public float quickTimeActiveTime = 1.5f;
    public float quickTimeCritWindow = 0.01f;
    public Transform discardText;
    public Transform scryPanel;
    public float quickTimeMultiplier = 1f;
    float elapsedTime = 0f;

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

    public LayerMask normalMask;
    public LayerMask characterMask;
    [HideInInspector] public bool multiDamageType = false;

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
        quickTimeEvent.gameObject.SetActive(false);
        discardText.gameObject.SetActive(false);
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
        scryPanel.gameObject.SetActive(false);
        itemContainer.gameObject.SetActive(false);
    }

    public void ShowQuickTimeEvent()
    {
        if(pattern != ""){
            AudioManager.Instance.PlaySoundEffect("Whoosh",1f);
            quickTimeEvent.gameObject.SetActive(true);
            quickTimeEvent.GetComponentInChildren<QuickTimeEvent>().Initialize(pattern);
            waitingForQuickTime = true;
        }
        else
        {
            AudioManager.Instance.PlaySoundEffect("Buff",1f);
        }
    }

    public void ShowScryPanel(PlayerCombatant player, int scryAmount)
    {
        scryPanel.gameObject.SetActive(true);
        waitingForInput = true;
        var cards = player.Scry(scryAmount);
        foreach(Card c in cards)
        {
            var cardDisplay = Instantiate(Resources.Load<GameObject>("DisplayCardPrefab"), scryPanel.Find("Panel")).GetComponent<CardDisplay>();
            cardDisplay.SetData(c);
            //add onclick listener
            cardDisplay.GetComponentInChildren<MenuCardDisplay>().onPointerDown = () => {
                    player.deck.Remove(cardDisplay.card);
                    player.discard.Add(cardDisplay.card);
                    Destroy(cardDisplay.gameObject);
            };
        }
    }

    public void HideScryPanel()
    {
        scryPanel.gameObject.SetActive(false);
        waitingForInput = false;
    }

    public void StartBattle(GameObject newplayer)
    {
        player = newplayer;
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
        //update spacing
        var enemies = combatants.Where(c => c.tag == "Enemy" && c.alive).ToList();
        for(int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];

            enemy.transform.localPosition = new Vector3((-0.5f * YourParty.instance.spacing * enemies.Count) + (YourParty.instance.spacing * i), 0f, 0f);
            enemy.GetComponent<Combatant>().startPosition = enemy.transform.position;
        }
    }


    public void ExecuteCard(Card card, Combatant caller)
    {
        print("Executing card: " + card.cardName);
        if(card.tpCost > 0) gainTP = false; else gainTP = true;
        if(activePlayer != null && gainTP) activePlayer.tp += 5; //Gain TERROR points
        foreach(var action in card.effects)
        {
            action.caller = caller;
            pattern = action.pattern;
            if(action.targetType == TargetType.AllEnemies){SelectTargets(GameObject.FindGameObjectsWithTag("Enemy").ToList().Select(go => go.GetComponent<Combatant>()).ToList());actionQueue.Add(action); ShowQuickTimeEvent();} 
            else if(action.targetType == TargetType.AllAllies){SelectTargets(GameObject.FindGameObjectsWithTag("PlayerCombatant").ToList().Select(go => go.GetComponent<Combatant>()).ToList());actionQueue.Add(action); ShowQuickTimeEvent();}
            else if(action.targetType == TargetType.Self){SelectTargets(new List<Combatant>(){caller});actionQueue.Add(action); ShowQuickTimeEvent();}
            else if(action.targetType == TargetType.None){SelectTargets(new List<Combatant>());actionQueue.Add(action); ShowQuickTimeEvent();}
            
            else{
                if (caller.HasStatusEffect("Insanity") != null)
                {
                    Debug.Log($"Caller: {action.caller}");
                    actionQueue.Add(action);
                    actionQueue.Add(action);
                }
                else
                {
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
    }

    public void SelectRandomTargets(Combatant caller, TargetType targetType)
    {
        
        List<Combatant> possibleTargets = new List<Combatant>();
        var targetTag = "PlayerCombatant";
        if(caller is PlayerCombatant) targetTag = "Enemy";
        Debug.Log($"Target tag: {targetTag}");

        if(caller.HasStatusEffect("E-S-Pow") != null)
        {
            GameManager.Instance.ShowMessage("Enemy is confused!");
            if(targetType == TargetType.SingleEnemy) targetType = TargetType.SingleAlly;
            else if(targetType == TargetType.SingleAlly) targetType = TargetType.SingleEnemy;
            else if(targetType == TargetType.AllEnemies) targetType = TargetType.AllAllies;
            else if(targetType == TargetType.AllAllies) targetType = TargetType.AllEnemies;
        }

        if(targetType == TargetType.SingleEnemy || targetType == TargetType.AllEnemies)
        {
            //players in activeCombatants
            possibleTargets = combatants.Where(c => c.tag == targetTag && c.alive).ToList();
            if(targetType == TargetType.AllEnemies){
                currentTargets = new List<Combatant>(possibleTargets);
                var tauntingTargets = possibleTargets.Where(c => c.HasStatusEffect("Taunt") != null).ToList();
                if (tauntingTargets.Count > 0)
                {
                    possibleTargets = tauntingTargets;
                }
                Transform spawnPoint = GameObject.Find("BattleSetup/PlayerSpawn").transform;
                SetPose(spawnPoint.transform, "", CameraAngle.wideBehind, "");
            }
            else
            {
                var tauntingTargets = possibleTargets.Where(c => c.HasStatusEffect("Taunt") != null).ToList();
                if (tauntingTargets.Count > 0)
                {
                    possibleTargets = tauntingTargets;
                }
                var target = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
                caller.SetTargetPosition(target.transform.position + target.transform.forward * 2f);
                currentTargets = new List<Combatant>() { target };
                SetPose(target.transform, "", CameraAngle.behind, "");
            }
        }
        else if(targetType == TargetType.SingleAlly || targetType == TargetType.AllAllies)
        {
            targetTag = "PlayerCombatant";
            if(caller is EnemyCombatant) targetTag = "Enemy";
            possibleTargets = combatants.Where(c => c.tag == targetTag && c.alive).ToList();
            if(targetType == TargetType.AllAllies){
                currentTargets = new List<Combatant>(possibleTargets);
                Transform spawnPoint = GameObject.Find("BattleSetup/EnemySpawn").transform;
                SetPose(spawnPoint.transform, "", CameraAngle.wideBehind, "");
            }
            else
            {
                var target = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
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
            Transform spawnPoint = GameObject.Find("BattleSetup/PlayerSpawn").transform;
            SetPose(spawnPoint.transform, "", CameraAngle.wideBehind, "");
        }
        else if(targetType == TargetType.Any)
        {
            possibleTargets = combatants;
            var target = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];
            currentTargets = new List<Combatant>() { target };
            SetPose(target.transform, "", CameraAngle.standard, "");
        }

        
    }

    void Win()
    {
        AudioManager.Instance.PlayMusic(Resources.Load<AudioClip>("Sound/Music/Victory"), 0.2f);
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
            float roll = UnityEngine.Random.Range(0f, 100f);
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

        //disable boss hp overlay
        var menu = FindFirstObjectByType<Menu>();
        menu.bossHP.gameObject.SetActive(false);
        DialogBox d = FindFirstObjectByType<DialogBox>();
        d.StartDialog(dialog);
        d.OnDialogFinished += OnDialogFinished;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    
    }

    void Lose()
    {
        AudioManager.Instance.PlayMusic(Resources.Load<AudioClip>("Sound/Music/Defeat"), 0.2f);
        DialogBox d = FindFirstObjectByType<DialogBox>();
        //disable boss hp overlay
        var menu = FindFirstObjectByType<Menu>();
        menu.bossHP.gameObject.SetActive(false);
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
        AudioManager.Instance.PlayMusic(Resources.Load<AudioClip>("Sound/Music/Lose"), 0.2f);
        d.OnDialogFinished += OnLoseDialogFinished;
    }

    void OnLoseDialogFinished()
    {
        if(player != null) player.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        YourParty.instance.LoadLastSave();
         var d = FindFirstObjectByType<DialogBox>();
        d.OnDialogFinished -= OnLoseDialogFinished;
        Destroy(gameObject);
    }

    void OnDialogFinished()
    {
        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
        //Destroy(cameraRig);
        GameManager.Instance.DestroyCamera();
        Destroy(gameObject);
        //unsubscribe
        var d = FindFirstObjectByType<DialogBox>();
        d.OnDialogFinished -= OnDialogFinished;
        onWin?.Invoke();
    }

    void Update()
    {
        if(canWin == false) return;

        if(waitingForQuickTime) return;
        quickTimeEvent.gameObject.SetActive(false);

        if(YourParty.instance.devTools){
             if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Win();
                canWin = false;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Lose();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                if(activeCombatant is PlayerCombatant p) p.GainTP(50);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                ShowBackground();
            }
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
           HideInventory();
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
        if(dodgeWindow > 0f) dodgeWindow -= Time.unscaledDeltaTime; else {dodgeInput = "";}

        //Dodge system
        if(canDodge)
        {
            if(dodgeCooldown <= 0f)
            {
                if (Input.GetKeyDown(KeyCode.A)){ dodgeInput = "Left";}
                if (Input.GetKeyDown(KeyCode.D)) dodgeInput = "Right";
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) dodgeInput = "Jump";
                if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.LeftShift)) dodgeInput = "Duck";
                //if(Input.GetKeyDown(KeyCode.Mouse1)) dodgeInput = "Block";

                if(dodgeInput != "")
                {
                    /*
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
                    */
                    AudioManager.Instance.PlaySoundEffect("Whoosh",UnityEngine.Random.Range(0.8f,1.2f));
                    dodgeWindow = dodgeInputWindow;
                    if(gameDifficulty.value == 0) dodgeWindow = dodgeInputWindow * 1.5f;
                    if(gameDifficulty.value == 2) dodgeWindow = dodgeInputWindow * 0.75f; 
                    dodgeCooldown = 0.5f;
                    if(gameDifficulty.value == 0) dodgeCooldown = 0.25f;
                    if(gameDifficulty.value == 2) dodgeCooldown = 1f;
                    foreach(var t in currentTargets) t.PlayAnimation(dodgeInput);
                }
            }
        }

        // Countdown when clock is active (> 0)
        if (!waitingForInput)
        {
            if(clock > 0f) clock -= Time.deltaTime;
            else if (actionQueue.Count > 0 && hitsRemaining <= 0)
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
                    if(attacksRemaining <= 0 || activeCombatant.alive == false)
                    {
                        NextTurn();
                        attacksRemaining = 1;
                    }else{
                        if(activePlayer != null){
                            activePlayer.BonusTurn();
                            itemContainer.gameObject.SetActive(false);
                            buttonContainer.gameObject.SetActive(true);
                        }
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

            icon.GetComponentInChildren<TMP_Text>().text = "";
            
                if(combatant is EnemyCombatant e && e.portrait != null)
                {
                        icon.GetComponentInChildren<Image>().sprite = e.portrait;
            }
            else
            {
                icon.GetComponentInChildren<Image>().sprite = Resources.Load<Sprite>($"Sprites/{combatant.combatantName}");
            }
            
        }
    }

    List<Combatant> SimulateNextTurns(int count)
{
    const float TURN_THRESHOLD = 100f;
    const int MAX_ITERATIONS = 10000;

    var sim = new List<(Combatant c, float initiative)>();
    foreach (var c in combatants.Where(c => c.alive))
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
                    if (chosen.c.surprise) //First turn initiative boost
                    {
                        chosen.c.surprise = false;
                        chosen.initiative = 0f;
                    }
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

    public void UpdateDiscardPower(int newPower)
    {   
        if(newPower <= 0)
        {
            discardText.gameObject.SetActive(false);
        }
        else
        {
            discardText.gameObject.SetActive(true);
            discardText.GetComponentInChildren<TMP_Text>().text = $"{newPower}";
        }
        discardPower = newPower;
    }

    public void NextTurn()
    {
        Camera.main.cullingMask = normalMask;
       cameraRig.transform.Find("CameraPosition/CutsceneCamera/Background").GetComponent<SpriteRenderer>().enabled = false;

        quickTimeMultiplier = 1f;
        pitch = 1f;
        itemContainer.gameObject.SetActive(false);
        buttonContainer.gameObject.SetActive(false);
        string pose = "FightingIdle";
        foreach(Combatant combatant in combatants)
        {
            if(combatant.alive){
                var defaultPose = combatant.GetComponentInChildren<DefaultPose>();
                if(defaultPose != null && defaultPose.combatIdle != "") pose = defaultPose.combatIdle;
                combatant.PlayAnimation(pose);
            }
            combatant.ReturnToStartPosition();
        }
        perfectDodge = true;
        playerStats.gameObject.SetActive(false);
        if(activeCombatant != null && activeCombatant.alive){activeCombatant.EndTurn();}
        if (combatants.Count == 0) return;
        Combatant current = SimulateNextTurns(1).FirstOrDefault();
        if (current == null) return;
        // Consume initiative
        current.initiative -= 100f;
        UpdateDiscardPower(0);
        current.StartTurn();
        hitCounter = 0;
        activeCombatant = current;
        pose = "FightingIdle";
        var defaultPose2 = current.GetComponentInChildren<DefaultPose>();
        if(defaultPose2 != null && defaultPose2.combatIdle != "") pose = defaultPose2.combatIdle;
        if(current is PlayerCombatant)
        {
            activePlayer = (PlayerCombatant)current;
            
            SetPose(current.transform, pose, CameraAngle.behind, "Mad");
            buttonContainer.gameObject.SetActive(true);
        }
        else
        {
            SetPose(current.transform, pose, CameraAngle.zoom, "Mad");
            activePlayer = null;
        }
        
        UpdateTurnOrderUI();
    }

    public void SkipTurn()
    {
        handManager.SetHandActive(false);
        NextTurn();
    }

    public void ShowBackground()
    {
        Camera.main.cullingMask = characterMask;
        var bg = cameraRig.transform.Find("CameraPosition/CutsceneCamera/Background").GetComponent<SpriteRenderer>();
        bg.enabled = true;
        bg.color = Color.red;
        if(activePlayer){
            var defaultPose = activePlayer.GetComponentInChildren<DefaultPose>();
            if(defaultPose != null)
            {
                bg.color = defaultPose.color;
            }
        }
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

    public void UseCokeKeg()
    {
        var action = new HealAction()
        {
            caller = activePlayer,
            targetType = TargetType.AllAllies,
            animation = "Drink",
            healAmount = "50"
        };
        GameManager.Instance.ShowMessage("Party: Started. Bass: Bumpin'. Health: Restored");
        actionQueue.Add(action);
        itemContainer.gameObject.SetActive(false);
        buttonContainer.gameObject.SetActive(false);
    }

    public void UseDrPepper()
    {
        var action = new ReviveAction()
        {
            caller = activePlayer,
            targetType = TargetType.SingleAlly,
            animation = "Drink",
        };
        var targetAction = new ChooseTargetsAction()
        {
            targetType = action.targetType,
            prompt = "Revive Who?",
            targetDead = true,
            gameAction = action,
            caller = activePlayer
        };
        GameManager.Instance.ShowMessage("Who Will Revive?");
        actionQueue.Add(targetAction);
        itemContainer.gameObject.SetActive(false);
        buttonContainer.gameObject.SetActive(false);
    }

    public void UseCoffee()
    {
        var action = new GainMPAction()
        {
            caller = activePlayer,
            targetType = TargetType.SingleAlly,
            animation = "Drink",
            mpAmount = "30"
        };
        var targetAction = new ChooseTargetsAction()
        {
            targetType = action.targetType,
            prompt = "Who Will Drink the Coffee?",
            gameAction = action,
            caller = activePlayer
        };
        GameManager.Instance.ShowMessage("Who Will Drink the Coffee?");
        actionQueue.Add(targetAction);
        itemContainer.gameObject.SetActive(false);
        buttonContainer.gameObject.SetActive(false);
    }

    public void UseBang()
    {
        var action = new DamageAction()
        {
            caller = activePlayer,
            targetType = TargetType.SingleEnemy,
            animation = "Throw",
            damageType = DamageType.Psychic,
            damage = "25",
            hits = 1,
            bonusActions = 1
        };
        var targetAction = new ChooseTargetsAction()
        {
            targetType = action.targetType,
            prompt = "Throw the Bang",
            gameAction = action,
            caller = activePlayer
        };
        GameManager.Instance.ShowMessage("Who Will Drink the Coke?");
        actionQueue.Add(targetAction);
        attacksRemaining ++;
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

    public void HideInventory()
    {
         if(itemContainer.gameObject.activeInHierarchy)
            {
                itemContainer.gameObject.SetActive(false);
                buttonContainer.gameObject.SetActive(true);
                activePlayer.BonusTurn();
            }
    }

    public void SpawnProjectile(Combatant caller, string prefab = "")
    {
        GameObject projectileInstance = null;
        if(prefab != "") projectileInstance = Resources.Load<GameObject>(prefab); else projectileInstance = Resources.Load<GameObject>("Projectile");
        var p = Instantiate(projectileInstance,caller.gameObject.transform.position,Quaternion.identity);
        var projectile = p.GetComponent<Projectile>();
        if(caller is PlayerCombatant) projectile.Initialize("Enemy"); else projectile.Initialize("PlayerCombatant");
    }

    public void PlayerHit()
    {
        hitsRemaining --;
        hitCounter ++;
        //damage all targets
        foreach(var t in currentTargets)
        {
            if(t.alive){
                print($"QuickTime Multiplier = {quickTimeMultiplier} Pending Damage = {pendingDamage}");
                CameraShake(1.5f,0.4f);
                var invincible = t.HasStatusEffect("Block");
                if(invincible != null)
                {
                    invincible.amount -= 1;
                    t.PlayAnimation("BlockSuccess");
                    AudioManager.Instance.PlaySoundEffect("Parry");
                    if(invincible.amount <= 0) t.RemoveStatusEffect("Block");
                }else{
                    var d = t.TakeDamage(activeCombatant,(int)pendingDamage * quickTimeMultiplier, pendingDamageType);
                    var effect = Instantiate(Resources.Load<GameObject>("Particles/Hit"), t.transform);
                    
                        {
                            if(hitsRemaining == 0) t.PlayAnimation("Knockdown");
                                else t.PlayAnimation("Stunned");
                        }
                
                    if(pendingDamageType == DamageType.Slashing)
                        AudioManager.Instance.PlaySoundEffect("HitSlash",UnityEngine.Random.Range(0.8f,1.2f));
                    if(pendingDamageType == DamageType.Bludgeoning)
                        AudioManager.Instance.PlaySoundEffect("s_punch",UnityEngine.Random.Range(0.8f,1.2f));
                    if(pendingDamageType == DamageType.Psychic)
                        AudioManager.Instance.PlaySoundEffect("Crackle",UnityEngine.Random.Range(0.8f,1.2f));
                    if(pendingDamageType == DamageType.None)
                        AudioManager.Instance.PlaySoundEffect("Crackle",UnityEngine.Random.Range(0.8f,1.2f));
                    if(lifestrike){lifestrike = false; activeCombatant.Heal(d);}
                    var tpGain = Mathf.Clamp(d/4f,10,30);
                    if(activePlayer != null && gainTP) activePlayer.tp += (int)(tpGain); //Gain TERROR points based on damage dealt

                    if(pendingStatusEffect != null){ t.ApplyStatusEffect(pendingStatusEffect);}
                    if (activeCombatant.HasStatusEffect("Poisoner") != null)
                    {
                        t.ApplyStatusEffect(CardDatabase.Instance.getStatusEffect("Poisoned",1,1));
                    }
                    if (t.HasStatusEffect("Bounty") != null)
                    {
                        attacksRemaining += 1;
                        activePlayer.GainMP(30);
                        GameManager.Instance.ShowMessage("Bounty claimed! Bonus action");
                    }
                }
                if(multiDamageType){if(pendingDamageType == DamageType.Slashing) pendingDamageType = DamageType.Bludgeoning;
                else if(pendingDamageType == DamageType.Bludgeoning) pendingDamageType = DamageType.Psychic;
                else if(pendingDamageType == DamageType.Psychic) pendingDamageType = DamageType.Slashing;}
            }
            
        }
        if(hitsRemaining > 0 && loopAnimation)
            {
                activeCombatant.RestartAnimation();
            }
        if(hitsRemaining <= 0)
        {
            loopAnimation = false;
            pendingStatusEffect = null;  
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
            CameraShake(0.4f,0.2f);
           AudioManager.Instance.PlaySoundEffect("OrchestraHit",pitch);
           pitch += 0.1f;
            foreach(var t in currentTargets)
            {
                if(t.alive){
                var effect = Instantiate(Resources.Load<GameObject>("Particles/Block"), t.transform);
                }
            }
        }
        else if(dodgeInput == "Block")
        {
            pendingDamage *= 0.5f; //Take half damage on block
            dodgeCooldown = 0;
            dodgeInput = "";
            perfectDodge = false;
            AudioManager.Instance.PlaySoundEffect("Anvil",UnityEngine.Random.Range(0.8f,1.2f));
            foreach(var t in currentTargets)
            {
                if(t.alive){
                    var effect = Instantiate(Resources.Load<GameObject>("Particles/Block"), t.transform);
                    t.PlayAnimation("BlockSuccess");
                    t.TakeDamage(activeCombatant,(int)pendingDamage, pendingDamageType);
                }
            }
        }
        else{
            perfectDodge = false;
            CameraShake(1f,0.4f);
            if(activeCombatant is EnemyCombatant e)
            {
                e.OnHitSuccess();
            }
            foreach(var t in currentTargets)
            {
                if(t.alive){
                     var invincible = t.HasStatusEffect("Block"); 
                    if(invincible != null)
                    {
                        t.PlayAnimation("BlockSuccess");
                        AudioManager.Instance.PlaySoundEffect("Parry");
                        invincible.amount -= 1;
                        if(invincible.amount <= 0) t.RemoveStatusEffect("Block");
                    }else{
                        var effect = Instantiate(Resources.Load<GameObject>("Particles/Hit"), t.transform);
                        CameraShake(1f,0.2f);
                        t.TakeDamage(activeCombatant,(int)pendingDamage, pendingDamageType);
                        if(currentTargets.Count > 0 && currentTargets[0] == activeCombatant)
                        {
                            activeCombatant.RemoveStatusEffect("E-S-Pow");
                            GameManager.Instance.ShowMessage($"{activeCombatant.combatantName} hits themself!");
                        }
                        else{
                            if(hitsRemaining == 0) t.PlayAnimation("Knockdown");
                            else t.PlayAnimation("Stunned");
                        }
                        if(pendingDamageType == DamageType.Slashing)
                        AudioManager.Instance.PlaySoundEffect("HitSlash",UnityEngine.Random.Range(0.8f,1.2f));
                        if(pendingDamageType == DamageType.Bludgeoning)
                            AudioManager.Instance.PlaySoundEffect("s_punch",UnityEngine.Random.Range(0.8f,1.2f));
                        if(pendingDamageType == DamageType.Psychic)
                            AudioManager.Instance.PlaySoundEffect("Crackle",UnityEngine.Random.Range(0.8f,1.2f));
                        if(pendingStatusEffect != null && pendingStatusEffect.name != "")
                        {
                            if(t.alive)
                            t.ApplyStatusEffect(pendingStatusEffect);
                        }
                    }
                }
            }
        }
        hitsRemaining --;
        if(hitsRemaining > 0 && loopAnimation)
        {
            activeCombatant.RestartAnimation();
        }
        if(hitsRemaining <= 0)
        {
            if(perfectDodge && actionQueue.Count == 0) //you dodged perfectly and there are no more actions queued
            {
                AudioManager.Instance.PlaySoundEffect("SwordClang");
                AudioManager.Instance.PlaySoundEffect("Counter");
                AudioManager.Instance.PlaySoundEffect("Parry",UnityEngine.Random.Range(0.8f,1.2f));
                GameManager.Instance.ShowMessage($"<color=yellow>Counter!</color>");
                foreach(var t in currentTargets)
                {
                    if(t is PlayerCombatant pt && t.alive){
                        var increasedCounter = pt.HasStatusEffect("Increased Counter");
                        var counterDamage = "LEVEL*10";
                        if(increasedCounter != null){
                            counterDamage += $"+{increasedCounter.amount}";
                        }
                        var action = new CounterDamageAction()
                        {
                            caller = t,
                            animation = "SwordCounter",
                            damage = "LEVEL*10",
                            damageType = DamageType.Psychic,
                            hits = 1
                        };
                        
                        if(t.HasStatusEffect("Rock Solid") != null)
                        {
                            action.statusEffect = CardDatabase.Instance.getStatusEffect("Off-Balance");
                        }
                        actionQueue.Insert(0, action);
                        if(gainTP) pt.tp += 5; //Gain TERROR points based on damage dealt
                    }
                    if(activeCombatant is PlayerCombatant p)
                    {
                        SelectTargets(combatants.Where(c => c is EnemyCombatant).ToList()); //counter the counter
                    }else{
                        SelectTargets(new List<Combatant>() { activeCombatant });
                    }
                }
                pendingStatusEffect = null;
                EndAction();
                clock = 0;
            }
            else
            {
                pendingStatusEffect = null;
                EndAction();
            }
            
        }
    }

    public void CameraShake(float intensity, float duration)
    {
       //camera shake using cinemachine
         var impulseSource = cameraRig.GetComponent<CinemachineImpulseSource>();
        if(impulseSource != null)        {
            impulseSource.GenerateImpulse(intensity);  
        }
    }

    public void SetPose(Transform target, string pose, CameraAngle cameraAngle, string face)
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
        else if(cameraAngle == CameraAngle.super) cameraAnimator.Play("Camera_Super");
        else if(cameraAngle == CameraAngle.lockOn) cameraAnimator.Play("Camera_LockOn");
        else if(cameraAngle == CameraAngle.counter) cameraAnimator.Play("Camera_Counter");
    }
}