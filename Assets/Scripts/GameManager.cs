using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
public enum GameplayState{FreeMovement, RestrictedMovement, Dialog, Combat}
[System.Serializable] public class InventoryItem
{
    public string itemName;
    public int quantity;
    public string description;

    public string function = "EquipItem";
    public InventoryItem(string name, int qty)
    {
        itemName = name;
        quantity = qty;
    }
    public GameAction gameAction;
    public System.Action<Menu> outOfBattleAction;
}

public class Equipment : InventoryItem
{
    public StatusEffect[] statusEffects;
    public string type;
    public Equipment(string name, int qty) : base(name, qty)
    {
        function = ""; // Equipment doesn't use functions
    }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    GameplayState gameplayState;
    float freeze = 0f;
    public int currentSpawnPointIndex = 0;
    public int sceneVariant = 0;
    public int maxActiveBots = 2;
    [HideInInspector] public List<string> quests = new List<string>();
    public List<InventoryItem> inventory = new List<InventoryItem>();
    Transform battleHUD;
    GameObject ui;
    TMP_Text message;
    Material skyboxMaterial;
    GameObject[] sceneVariants;
    float messageTimer = 0f;
    GameObject cameraRig;
    RectTransform inventoryUI;
    public float playTime = 0f;
    Menu menu;

    public List<string> finishedEncounters = new List<string>();

    public InventoryItem GetInventoryItemByName(string name){
        InventoryItem newItem = new InventoryItem(name,1);
        var battleManager = FindFirstObjectByType<BattleManager>();
        
        switch(name){
            case "Coke":
                newItem = new InventoryItem(name,1);
                newItem.description = "Good for your HEALTH... You think. Can be used in combat";
                newItem.outOfBattleAction = (menu) => {
                    var pm = YourParty.instance.GetPartyMember(menu.currentCharacter);
                    if(pm != null)
                    {
                        var multiplier = 10f; 
                        if(pm.subClass == CardClass.Grappler) multiplier = 20f; 
                        if(pm.mainClass == CardClass.Grappler) multiplier = 30f;
                        var maxHp = pm.level * multiplier + 50f;
                        float healAmount = 50f;
                        pm.hpPercentage += healAmount / maxHp;
                        if(pm.hpPercentage > 1f) pm.hpPercentage = 1f;
                    }
                };
                newItem.gameAction = new HealAction()
                {
                    targetType = TargetType.SingleAlly,
                    animation = "Drink",
                    healAmount = "50"
                };
            break;
            case "Bang":
                newItem = new InventoryItem(name,1);
                newItem.description = "Throw at an enemy to deal damage. Does not consume a turn";
                newItem.gameAction = new DamageAction()
                {
                    targetType = TargetType.SingleEnemy,
                    animation = "Throw",
                    damageType = DamageType.Psychic,
                    damage = "25",
                    hits = 1
                };
            break;
            case "Lockpick":
                newItem = new InventoryItem(name,1);
                newItem.description = "You made this out of a hairpin! Perks of having long, luscious locks";
            break;
            case "Coffee":
                newItem = new InventoryItem(name,1);
                newItem.description = "Gives you the jitters, the craps, and 30 MP. You drink it BLACK, because you are A REAL MAN";
                newItem.gameAction = new GainMPAction()
                {
                    targetType = TargetType.SingleAlly,
                    animation = "Drink",
                    mpAmount = "30"
                };
            break;
            case "DrPepper":
                newItem = new InventoryItem(name,1);
                newItem.description = "The Dr is In! Instantly revives an ally and restores 50 HP";
                newItem.gameAction = new ReviveAction()
                {
                    targetType = TargetType.SingleAlly,
                    animation = "Drink"
                };
            break;
            case "Coca-Cola Keg":
                newItem = new InventoryItem(name,1);
                newItem.description = "Now with 3000 calories! Enough health for the whole party to drink at once!";
                newItem.outOfBattleAction = (menu) => {
                    foreach(PartyMember pm in YourParty.instance.reserve)
                    {
                        var multiplier = 10f; 
                        if(pm.subClass == CardClass.Grappler) multiplier = 20f; 
                        if(pm.mainClass == CardClass.Grappler) multiplier = 30f;
                        var maxHp = pm.level * multiplier + 50f;
                        float healAmount = 50f;
                        pm.hpPercentage += healAmount / maxHp;
                        if(pm.hpPercentage > 1f) pm.hpPercentage = 1f;
                    }
                };
                newItem.gameAction = new HealAction()
                {
                    targetType = TargetType.AllAllies,
                    animation = "Drink",
                    healAmount = "50"
                };
            break;
            case "Leather Vest":
                newItem = new Equipment(name,1)
                {
                    description = "+20 Max HP",
                    statusEffects = new StatusEffect[]
                    {
                        new StatusEffect()
                        {
                            name = "Armored",
                            stat = "MAXHP",
                            amount = 20,
                            duration = -1
                        },
                    },
                    type = "Body"
                };
            break;
            case "Viking Helmet":
                newItem = new Equipment(name,1)
                {
                    description = "+5 Attack",
                    statusEffects = new StatusEffect[]
                    {
                        new StatusEffect()
                        {
                            name = "BrassKnuckles",
                            stat = "ATK",
                            amount = 5,
                            duration = -1
                        }
                    },
                    type = "Head"
                };
            break;
            case "Brass Knuckles":
                newItem = new Equipment(name,1)
                {
                    description = "+5 Attack",
                    statusEffects = new StatusEffect[]
                    {
                        new StatusEffect()
                        {
                            name = "BrassKnuckles",
                            stat = "ATK",
                            amount = 5,
                            duration = -1
                        }
                    },
                    type = "Accessory"
                };
            break;
            case "Bicycle Helmet":
                newItem = new Equipment(name,1)
                {
                    description = "+20 Max HP",
                    statusEffects = new StatusEffect[]
                    {
                        new StatusEffect()
                        {
                            name = "Helmet",
                            stat = "MAXHP",
                            amount = 20,
                            duration = -1
                        },
                    },
                    type = "Head"
                };
            break;
            case "Mind Helmet":
                newItem = new Equipment(name,1)
                {
                    description = "Protects against UFO's and rogue airwaves. No protection against judgement. +5 MP/turn",
                    statusEffects = new StatusEffect[]
                    {
                        new StatusEffect()
                        {
                            name = "Mind Helmet",
                            stat = "PSY",
                            amount = 5,
                            duration = -1
                        }
                    },
                    type = "Head"
                };
            break;
            case "Ninja Headband":
                newItem = new Equipment(name,1)
                {
                    description = "+5 Speed (but only if you run with your hands behind you)",
                    statusEffects = new StatusEffect[]
                    {
                        new StatusEffect()
                        {
                            name = "Headband",
                            stat = "SPD",
                            amount = 5,
                            duration = -1
                        }
                    },
                    type = "Head"
                };
            break;
        }
        return newItem;
    }

    public void SetGameplayState(GameplayState newState)
    { 
        gameplayState = newState; 
        if(battleHUD != null)
        {
            if(gameplayState == GameplayState.Combat)
            {
                battleHUD.gameObject.SetActive(true);
            }
            else
            {
                battleHUD.gameObject.SetActive(false);
            }
        }
    }

    public void Freeze(float t){freeze = t;}
    public bool IsFrozen(){return (freeze > 0f);}

    public GameplayState GetGameplayState(){return gameplayState;}

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        // Set the singleton reference
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
             Object.Instantiate(Resources.Load<GameObject>("AudioManager"));
    }

    void Start()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        //message = GameObject.Find("MainCanvas/OtherHUD/Message").GetComponent<TMP_Text>();
         sceneVariants = GameObject.FindGameObjectsWithTag("SceneVariant");
        ChangeSceneVariant();
        if(EventSystem.current == null)
            Object.Instantiate(Resources.Load<GameObject>("EventSystem"));
        menu = FindFirstObjectByType<Menu>();
   
    }

    public GameObject GetCamera(out Animator cameraAnimator, out CinemachineCamera cutsceneCamera)
    {
        if(cameraRig == null)
        {
            cameraRig = Instantiate(Resources.Load<GameObject>("CameraRig"));
        }

        cameraAnimator = cameraRig.GetComponent<Animator>();
        cutsceneCamera = cameraRig.GetComponentInChildren<CinemachineCamera>();
        // Give the cutscene camera a high priority so it wins over gameplay vcams while active
        cutsceneCamera.Priority = 10;
        cameraRig.transform.SetParent(null);
        cameraRig.transform.localPosition = Vector3.zero;
        cameraRig.transform.localRotation = Quaternion.identity;
        return cameraRig;
    }

    public void DestroyCamera()
    {
        print("Destorying camera");
        if (cameraRig == null) return;

        var cutsceneCamera = cameraRig.GetComponentInChildren<CinemachineCamera>();
        if (cutsceneCamera != null)
            cutsceneCamera.Priority = 0;

        cameraRig.transform.SetParent(null);
        cameraRig.transform.localPosition = Vector3.zero;
        cameraRig.transform.localRotation = Quaternion.identity;
    }


    public IEnumerator Fade(bool toBlack, Transform cameraTarget = null)
    {
        if(toBlack)
        {
        var player = GameObject.FindGameObjectWithTag("Player").transform;
        //player.GetComponentInChildren<Animator>().Play("ArmsCrossed");
        }
        var ui = GameObject.Find("MainCanvas");
        var imgToFade = ui.transform.Find("OtherHUD/Black").GetComponent<UnityEngine.UI.Image>();
        if(cameraTarget != null && toBlack)
        {
            var cam = GetCamera(out var cameraAnimator, out var cutsceneCamera);
            cam.transform.position = cameraTarget.position;
            // Ensure this cutscene camera has highest priority during the fade
            cutsceneCamera.Priority = 100;
            cameraAnimator.Play("Camera_Behind");
        }
        //Fade over the course of 1s
        float duration = 1f;
        float elapsed = 0f;     
        Color startColor = imgToFade.color;
        if(!toBlack){startColor = new Color(0,0,0,1); imgToFade.color = startColor;}
        Color targetColor = toBlack ? Color.black : new Color(0,0,0,0);
        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if(imgToFade == null) yield break;
            imgToFade.color = Color.Lerp(startColor, targetColor, elapsed / duration);
            yield return null;
        }
        yield return new WaitForSeconds(0.1f);
        
    }

    void Update()
    {


        if(messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if(messageTimer <= 0f)
            {
                message.text = "";
            }
        }

        playTime += Time.deltaTime;

    }

    public void ShowMessage(string msg)
    {
        if(message == null) return;
        message.text += msg + "\n";
        messageTimer = 3f;
    }

    public void StartSceneTransition(string sceneName, int spawnPointIndex, int sceneVariant, Transform cameraTarget = null, Material skyboxMaterial = null)
    {
        StartCoroutine(SceneTransition(sceneName, spawnPointIndex, sceneVariant, cameraTarget, skyboxMaterial));
    }

    public IEnumerator SceneTransition(string sceneName, int spawnPointIndex, int newSceneVariant, Transform cameraTarget, Material skyboxMaterial)
    {
        yield return StartCoroutine(Fade(true, cameraTarget));
        ChangeScene(sceneName, spawnPointIndex, newSceneVariant, skyboxMaterial);
        yield return new WaitForSeconds(0.1f);
        Debug.Log("SceneTransition complete");
        yield return StartCoroutine(Fade(false, cameraTarget));
    }

    public void SetSpawnPoint(int spawnPointIndex)
    {
        currentSpawnPointIndex = spawnPointIndex;
    }

    public void ChangeScene(string sceneName, int spawnPointIndex, int newSceneVariant, Material sbMaterial = null)
    {
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        currentSpawnPointIndex = spawnPointIndex;
        sceneVariant = newSceneVariant;
        skyboxMaterial = sbMaterial;
        if(sceneName != "")
        {
            Debug.Log($"Changing scene to {sceneName} at spawn point {spawnPointIndex}");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        DisableFinishedEncounters();
        sceneVariants = GameObject.FindGameObjectsWithTag("SceneVariant");
        ChangeSceneVariant();
        
        SpawnPlayer(currentSpawnPointIndex);
    }

    void ChangeSceneVariant()
    {
        foreach (var sv in sceneVariants)
        {
            sv.SetActive(sv.name.EndsWith($"_{sceneVariant}"));
        }
    }

    public void AddEncounter(string encounterID)
    {
        if (!finishedEncounters.Contains(encounterID))
        {
            finishedEncounters.Add(encounterID);
        }
    }
    public void DisableFinishedEncounters()
    {
        var DisableEncounterObjects = FindObjectsByType<DisableEncounter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var po in DisableEncounterObjects)
        {
            po.enabled = true; //reset
            string poID = po.encounterID;
            if (finishedEncounters.Contains(poID))
            {
                po.Interact();
            }
        }
        foreach(ConditionalEncounter c in FindObjectsByType<ConditionalEncounter>(FindObjectsSortMode.None))
        {
            c.TryCheckConditions();
        }
    }

    public void SpawnPlayer(int spawnPointIndex)
    {
        var player = Object.Instantiate(Resources.Load<GameObject>("Player"));
        var cam = Object.Instantiate(Resources.Load<GameObject>("MainCamera"));
        PlayerInput playerInput = player.GetComponent<PlayerInput>();
        playerInput.cameraTransform = cam.transform;
        playerInput.enabled = true;
        var thirdPersonCam = Object.Instantiate(Resources.Load<GameObject>("ThirdPersonCamera"));
        var vcam = thirdPersonCam.GetComponent<CinemachineCamera>();
        //vcam.enabled = false;
        vcam.Follow = player.transform;
        vcam.LookAt = player.transform;
        vcam.enabled = true;
        var eventsystem = Object.Instantiate(Resources.Load<GameObject>("EventSystem"));
        ui = Object.Instantiate(Resources.Load<GameObject>("MainCanvas"));
        ui.name = "MainCanvas";
        message = ui.transform.Find("OtherHUD/Message").GetComponent<TMP_Text>();
        inventoryUI = ui.transform.Find("QuestHUD").GetComponent<RectTransform>();
        inventoryUI.gameObject.SetActive(false);
        UpdateQuests();
        var dialog = Object.Instantiate(Resources.Load<GameObject>("Dialog"));
        var spawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log("Found spawn points: " + spawnPoints.Length);
        //Skybox
        var skybox = Camera.main.GetComponent<Skybox>();
        if(skyboxMaterial != null){skybox.material = skyboxMaterial; skyboxMaterial = null;}
        else{ skybox.enabled = false;}
        foreach (var sp in spawnPoints)
        {
            if (sp.index == spawnPointIndex)
            {
                    player.GetComponent<CharacterController>().enabled = false;
                    player.transform.position = sp.transform.position;
                    player.transform.rotation = sp.transform.rotation;
                    player.GetComponent<CharacterController>().enabled = true;
            }
        }
        YourParty.instance.UpdateLeader();
    }

    public void AddInventoryItem(string itemName, int quantity)
    {
        ShowMessage($"Found: {itemName}");
        foreach(var item in inventory)
        {
            if(item.itemName == itemName)
            {
                item.quantity += quantity;
                return;
            }
        }
        inventory.Add(GameManager.Instance.GetInventoryItemByName(itemName));
    }
    public bool ConsumeInventoryItem(string itemName, bool consume, int quantity)
    {
        if(itemName == "") return true;
        foreach(var item in inventory)
        {
            if(item.itemName == itemName && item.quantity >= quantity)
            {
                if(consume){
                    item.quantity -= quantity;
                    if(item.quantity < 0) inventory.Remove(item);
                    ShowMessage($"Consumed: {itemName}");
                }
                return true;

            }
        }
        return false; //fail to find
    }
    

    public void AddQuest(string questName)
    {
        if(!quests.Contains(questName))
        {
            quests.Add(questName);
            ShowMessage($"New Quest: {questName} [Press ESCAPE to view]");
            UpdateQuests();
        }
    }

    public bool ConsumeQuest(string questName, bool consume)
    {
        if(quests.Contains(questName))
        {
            
            if(consume){ShowMessage($"Quest Complete: {questName}"); quests.Remove(questName);}
            UpdateQuests();
            return true;
        }
        return false;
    }

    void UpdateQuests()
    {
        if(inventoryUI == null) return;
        var questUI = inventoryUI.Find("CharacterContainer/Quests").GetComponent<TMP_Text>();
        
        questUI.text = "";
        foreach(var quest in quests)
        {
            questUI.text += "- " + quest + "\n";
        }
        
    }

}