using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Unity.VisualScripting;


public class BattleManager : MonoBehaviour
{
    public Action onWin;
    public List<Combatant> combatants = new List<Combatant>();
    public List<GameAction> actionQueue = new List<GameAction>();
    public bool loopAnimation = false;
    public float clock = 0f;
    public bool waitingForInput = false;
    public InputActionAsset inputActions;
    private InputAction cancelAction;
    private InputAction jumpAction;
    private InputAction horizontalAction;
    private InputAction verticalAction;
    private InputAction dpadLeftAction;
    private InputAction dpadRightAction;
    private InputAction dpadUpAction;
    private InputAction dpadDownAction;
    private bool inputActionsEnabled = false;
    public bool discardMode = false;
    float waitTime = 1f;
    public float pendingDamage = 0f;
    public DamageType pendingDamageType;
    public StatusEffect pendingStatusEffect;
    public PlayerCombatant activePlayer = null;
    public Combatant activeCombatant = null;
    public Combatant activeActionCaller = null;
    public RectTransform TurnOrderUI;
    public RectTransform playerStats;
    public Transform itemContainer;
    public Transform buttonContainer;
    public Transform discardPrompt;
    public int attacksRemaining = 1;
    public int hitsRemaining = 0;
    public bool canDodge = false;
    public bool perfectDodge = false;
    public int hitCounter = 0;
    public int discardPower = 0;
    public FloatValue gameDifficulty;
    bool gainTP = true;
    public bool dontPause = false;
    bool executingActions = false;
    bool canWin = true;
    public bool waitingForQuickTime = false;
    [HideInInspector] public GameObject pendingCardObject;
    [HideInInspector] public Card pendingCard;
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
    List<CardLootDrop> cardLootRewards = new List<CardLootDrop>();

    public LayerMask normalMask;
    public LayerMask characterMask;
    [HideInInspector] public bool multiDamageType = false;

    public TMP_Text deckSize;
    public TMP_Text discardSize;

    void Awake()
    {
        cameraRig = Instantiate(Resources.Load<GameObject>("CameraRig"));
        cameraRig.GetComponentInChildren<CinemachineCamera>().Priority = 8;
        cameraAnimator = cameraRig.GetComponentInChildren<Animator>();
        discardPrompt.gameObject.SetActive(false);
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
        InitializeInputActions();
        quickTimeEvent.gameObject.SetActive(false);
        discardText.gameObject.SetActive(false);
        foreach(var c in combatants)
        {
            if(c is EnemyCombatant enemy)
            {
                goldReward += enemy.goldReward;
                xpReward += enemy.xpReward;
                lootRewards.AddRange(enemy.lootDrops);
                cardLootRewards.AddRange(enemy.cardLootDrops);
            }
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        scryPanel.gameObject.SetActive(false);
        itemContainer.gameObject.SetActive(false);
    }

    private void InitializeInputActions()
    {
        if (inputActions == null || inputActionsEnabled)
            return;

        cancelAction = inputActions.FindAction("Cancel", false);
        jumpAction = inputActions.FindAction("Jump", false);
        horizontalAction = inputActions.FindAction("Horizontal", false);
        verticalAction = inputActions.FindAction("Vertical", false);
        dpadLeftAction = inputActions.FindAction("DpadLeft", false);
        dpadRightAction = inputActions.FindAction("DpadRight", false);
        dpadUpAction = inputActions.FindAction("DpadUp", false);
        dpadDownAction = inputActions.FindAction("DpadDown", false);

        var actions = new[]
        {
            cancelAction,
            jumpAction,
            horizontalAction,
            verticalAction,
            dpadLeftAction,
            dpadRightAction,
            dpadUpAction,
            dpadDownAction
        };

        foreach (var action in actions)
        {
            if (action != null)
                action.Enable();
        }

        inputActionsEnabled = true;
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
        if (cards == null) return;
        foreach(Card c in cards)
        {
            var cardDisplay = Instantiate(Resources.Load<GameObject>("DisplayCardPrefab"), scryPanel.Find("Panel")).GetComponent<CardDisplay>();
            cardDisplay.SetData(c);
            cardDisplay.displayMode = true;
            cardDisplay.onSubmitAction = () => {
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

    public void DiscardCard()
    {
        discardPrompt.GetComponent<TMP_Text>().text = $"PAY THE DISCARD COST";
        if(discardPower >= pendingCard.discardCost){
            discardMode = false;
            discardPrompt.gameObject.SetActive(false);
            ExecuteCard(pendingCard,activePlayer);
        }

    }


    public void ExecuteCard(Card card, Combatant caller)
    {
        print("Executing card: " + card.cardName);
        
        handManager.SetHandActive(false);
        
        foreach(var action in card.effects)
        {
            action.caller = caller;
            pattern = action.pattern;
            if(action.targetType == TargetType.AllEnemies){SelectTargets(GameObject.FindGameObjectsWithTag("Enemy").ToList().Select(go => go.GetComponent<Combatant>()).ToList());actionQueue.Add(action); ShowQuickTimeEvent(); ConsumeCard(card);} 
            else if(action.targetType == TargetType.AllAllies){SelectTargets(GameObject.FindGameObjectsWithTag("PlayerCombatant").ToList().Select(go => go.GetComponent<Combatant>()).ToList());actionQueue.Add(action); ShowQuickTimeEvent();ConsumeCard(card);}
            else if(action.targetType == TargetType.Self){SelectTargets(new List<Combatant>(){caller});actionQueue.Add(action); ShowQuickTimeEvent();ConsumeCard(card);}
            else if(action.targetType == TargetType.None){SelectTargets(new List<Combatant>());actionQueue.Add(action); ShowQuickTimeEvent();ConsumeCard(card);}
            
            else{
                if (caller.HasStatusEffect("Insanity") != null)
                {
                    Debug.Log($"Caller: {action.caller}");
                    actionQueue.Add(action);
                    actionQueue.Add(action);
                    ConsumeCard(card);
                }
                else
                {
                    var targetAction = new ChooseTargetsAction()
                    {
                        targetType = action.targetType,
                        prompt = "Choose your target",
                        gameAction = action,
                        caller = caller,
                        card = card
                    };
                    GameManager.Instance.ShowMessage("Choose your target for " + card.cardName);
                    actionQueue.Add(targetAction);
                }
            }
            UpdateDeckSize();
        }
    }

    public void UpdateDeckSize(){
        if(activePlayer != null)
        {
            deckSize.text = $"{activePlayer.deck.Count}";
            discardSize.text = $"{activePlayer.discard.Count}";
        }
        
    }

    public void ConsumeCard(Card card)
    {
        if(activePlayer != null && card != null){
            activePlayer.hand.Remove(card);
            activePlayer.discard.Add(card);
            activePlayer.mp = Mathf.Max(0, activePlayer.mp - card.cost);
            activePlayer.tp = Mathf.Max(0, activePlayer.tp - card.tpCost);
            if(card.tpCost > 0) gainTP = false; else gainTP = true;
            if(gainTP) activePlayer.tp += 5; //Gain TERROR points
            BattleManager.Instance.activePlayer.ShowStats();
            BattleManager.Instance.UpdateDiscardPower(BattleManager.Instance.discardPower - card.discardCost);
            ShowQuickTimeEvent();
            if(pendingCardObject != null)
            {
                handManager.cardsInHand.Remove(pendingCardObject);
                Destroy(pendingCardObject);
                pendingCardObject = null;
            }
        }
        UpdateDeckSize();
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
        Camera.main.cullingMask = normalMask;
       cameraRig.transform.Find("CameraPosition/CutsceneCamera/Background").GetComponent<SpriteRenderer>().enabled = false;
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
        foreach(var loot in cardLootRewards)
        {
            float roll = UnityEngine.Random.Range(0f, 100f);
            Card card = CardDatabase.Instance.GetCardByName(loot.cardName);
            if(roll <= loot.dropChance && card != null && card.cardClass == CardClass.None)
            {
                YourParty.instance.AddClasslessCard(loot.cardName, loot.quantity);
                dialog.Add(new Dialog()
                {
                    name = "",
                    text = $"New card looted: {loot.cardName} (x{loot.quantity})!",
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
        Camera.main.cullingMask = normalMask;
       cameraRig.transform.Find("CameraPosition/CutsceneCamera/Background").GetComponent<SpriteRenderer>().enabled = false;
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

        var cancelPressed = Input.GetKeyDown(KeyCode.Escape);
        if (!cancelPressed && cancelAction != null)
            cancelPressed = cancelAction.triggered;

        if (cancelPressed)
        {
            if (FindFirstObjectByType<Targeter>() == null)
            {
                HideInventory();
                //select "Use Item" button
                EventSystem.current.SetSelectedGameObject(buttonContainer.Find("Item").gameObject);
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
        if(dodgeWindow > 0f) dodgeWindow -= Time.unscaledDeltaTime; else {dodgeInput = "";}

        //Dodge system
        if(canDodge && currentTargets.Any(target => target is PlayerCombatant && target.alive))
        {
            float horizontal = 0f;
            float vertical = 0f;
            if (horizontalAction != null)
                horizontal += horizontalAction.ReadValue<float>();
            if (verticalAction != null)
                vertical += verticalAction.ReadValue<float>();
            if (dpadLeftAction != null)
                horizontal -= dpadLeftAction.ReadValue<float>();
            if (dpadRightAction != null)
                horizontal += dpadRightAction.ReadValue<float>();
            if (dpadUpAction != null)
                vertical += dpadUpAction.ReadValue<float>();
            if (dpadDownAction != null)
                vertical -= dpadDownAction.ReadValue<float>();
            if (Mathf.Abs(horizontal) > 1f)
                horizontal = Mathf.Sign(horizontal);
            if (Mathf.Abs(vertical) > 1f)
                vertical = Mathf.Sign(vertical);

            if(dodgeCooldown <= 0f)
            {
                var newDodgeInput = "";
                if (Input.GetKeyDown(KeyCode.A) || (dpadLeftAction != null && dpadLeftAction.WasPressedThisFrame()))
                    newDodgeInput = "Left";
                else if (Input.GetKeyDown(KeyCode.D) || (dpadRightAction != null && dpadRightAction.WasPressedThisFrame()))
                    newDodgeInput = "Right";
                else if (Input.GetKeyDown(KeyCode.W) || (dpadUpAction != null && dpadUpAction.WasPressedThisFrame()))
                    newDodgeInput = "Jump";
                else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.LeftShift) || (dpadDownAction != null && dpadDownAction.WasPressedThisFrame()))
                    newDodgeInput = "Duck";

                if(newDodgeInput != "")
                {
                    dodgeInput = newDodgeInput;
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
                    if(gameDifficulty.value == 0) dodgeCooldown = 0.35f;
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
                    var attackFullyResolved = actionQueue.Count == 0 && hitsRemaining <= 0;
                    var playerCannotContinue = activePlayer != null && attackFullyResolved &&
                        (!activePlayer.HasAffordableCard());
                    if(activeCombatant.alive == false ||
                        (activePlayer == null && attacksRemaining <= 0) ||
                        (activePlayer != null && playerCannotContinue))
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
        if (activeCombatant is PlayerCombatant outgoingPlayer)
        {
            outgoingPlayer.mp = 0;
        }
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
        if (activePlayer != null && (activePlayer.mp <= 0 || !activePlayer.HasAffordableCard()))
        {
            Invoke(nameof(NextTurn), 0.1f);
        }
        BattleManager.Instance.UpdateDeckSize();
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


    public void ShowItemDisplay()
    {
        itemContainer.gameObject.SetActive(true);
        var inventory = itemContainer.GetComponentInChildren<Inventory>();
        inventory.UpdateInventoryImages(GameManager.Instance.inventory);
        var activeCardDisplays = FindObjectsByType<CardDisplay>(FindObjectsSortMode.None);
        buttonContainer.gameObject.SetActive(false);
        handManager.SetHandActive(false);
        GameManager.Instance.SelectDefault();
        dontPause = true;
    }

    public void HideInventory()
    {
        dontPause = false;
         if(itemContainer.gameObject.activeInHierarchy)
            {
                itemContainer.gameObject.SetActive(false);
                buttonContainer.gameObject.SetActive(true);
                activePlayer.BonusTurn();
                GameManager.Instance.SelectDefault();
            }
    }

    public void SpawnProjectile(Combatant caller, string prefab = "")
    {
        GameObject projectileInstance = null;
        if(prefab != "") projectileInstance = Resources.Load<GameObject>(prefab); else projectileInstance = Resources.Load<GameObject>("Projectile");
        var p = Instantiate(projectileInstance,caller.gameObject.transform.position,Quaternion.identity);
        var projectile = p.GetComponent<Projectile>();
       
         //Check for E-S-Pow status effect
        if(caller.HasStatusEffect("E-S-Pow") != null)
        {
             if(caller is PlayerCombatant) projectile.Initialize("PlayerCombatant"); else projectile.Initialize("Enemy");
        }
        else
        {
             if(caller is PlayerCombatant) projectile.Initialize("Enemy"); else projectile.Initialize("PlayerCombatant");
        }
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
                    //Check bounty before hit
                    if (t.HasStatusEffect("Bounty") != null)
                    {
                        if(activePlayer != null){
                            attacksRemaining += 1;
                            activePlayer.GainMP(2);
                            GameManager.Instance.ShowMessage("Bounty claimed! +2 MP");
                        }
                    }
                    //Take damage
                    var damageCaller = activeActionCaller != null ? activeActionCaller : activeCombatant;
                    var d = t.TakeDamage(damageCaller,(int)pendingDamage * quickTimeMultiplier, pendingDamageType);
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
                    if(lifestrike){lifestrike = false; damageCaller.Heal(d);}
                    var tpGain = Mathf.Clamp(d/4f,10,30);
                    if(activePlayer != null && gainTP) activePlayer.tp += (int)(tpGain); //Gain TERROR points based on damage dealt

                    if(pendingStatusEffect != null){ t.ApplyStatusEffect(pendingStatusEffect);}
                    if (activeCombatant.HasStatusEffect("Poisoner") != null)
                    {
                        t.ApplyStatusEffect(CardDatabase.Instance.getStatusEffect("Poisoned",1,1));
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
            
        }else{
            
            CameraShake(1f,0.4f);
            if(activeCombatant is EnemyCombatant e)
            {
                e.OnHitSuccess();
            }
            foreach(var t in currentTargets)
            {
                if(t.alive){
                    var block = t.HasStatusEffect("Block");
                    if(block != null)
                    {
                        t.PlayAnimation("BlockSuccess");
                        AudioManager.Instance.PlaySoundEffect("Parry");
                        block.amount -= 1;
                        if(block.amount <= 0) t.RemoveStatusEffect("Block");
                    }
                    else
                    {
                        perfectDodge = false;
                    }
                    var effect = Instantiate(Resources.Load<GameObject>("Particles/Hit"), t.transform);
                    CameraShake(1f,0.2f);
                    var damageCaller = activeActionCaller != null ? activeActionCaller : activeCombatant;
                    t.TakeDamage(damageCaller,(int)pendingDamage, pendingDamageType);
                    if(currentTargets.Count > 0 && currentTargets[0] == activeCombatant)
                    {
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
        else if(cameraAngle == CameraAngle.knifeView) cameraAnimator.Play("Camera_KnifeView");
    }
}