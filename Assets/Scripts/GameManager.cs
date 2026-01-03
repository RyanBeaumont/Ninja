using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.EventSystems;
public enum GameplayState{FreeMovement, RestrictedMovement, Dialog, Combat}
public class InventoryItem
{
    public string itemName;
    public int quantity;

    public InventoryItem(string name, int qty)
    {
        itemName = name;
        quantity = qty;
    }
}
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    GameplayState gameplayState;
    float freeze = 0f;
    int currentSpawnPointIndex = 0;
    int sceneVariant = 0;
    public int maxActiveBots = 2;
    [HideInInspector] public int cash = 0;
    [HideInInspector] public List<string> quests = new List<string>();
    List<InventoryItem> inventory = new List<InventoryItem>();
    Transform battleHUD;
    GameObject ui;
    TMP_Text message;
    GameObject[] sceneVariants;
    float messageTimer = 0f;
    RectTransform inventoryUI;

    public List<string> finishedEncounters = new List<string>();

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
                print("Hiding Battle HUD");
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
           DestroyImmediate(gameObject);
            return;
        }

        // Set the singleton reference
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        message = GameObject.Find("MainCanvas/OtherHUD/Message").GetComponent<TMP_Text>();
         sceneVariants = GameObject.FindGameObjectsWithTag("SceneVariant");
        print("found " + sceneVariants.Length + " scene variants");
        ChangeSceneVariant();
        if(EventSystem.current == null)
            Object.Instantiate(Resources.Load<GameObject>("EventSystem"));
    }

    public IEnumerator Fade(bool toBlack)
    {
        var ui = GameObject.Find("MainCanvas");
        var imgToFade = ui.transform.Find("OtherHUD/Black").GetComponent<UnityEngine.UI.Image>();
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
        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
        
    }

    void Update()
    {
        if(freeze > 0f)
        {
            freeze -= Time.deltaTime;
        }

        UpdateBotActivation();

        if(Input.GetKeyDown(KeyCode.I))
        {
            inventoryUI.gameObject.SetActive(!inventoryUI.gameObject.activeSelf);
        }

        if(messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if(messageTimer <= 0f)
            {
                message.text = "";
            }
        }

    }

    public void ShowMessage(string msg)
    {
        message.text += msg + "\n";
        messageTimer = 3f;
    }

    public IEnumerator SceneTransition(string sceneName, int spawnPointIndex, int newSceneVariant)
    {
        yield return StartCoroutine(Fade(true));
        ChangeScene(sceneName, spawnPointIndex, newSceneVariant);
        yield return new WaitForSeconds(0.1f);
        yield return StartCoroutine(Fade(false));
    }

    public void ChangeScene(string sceneName, int spawnPointIndex, int newSceneVariant)
    {
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        currentSpawnPointIndex = spawnPointIndex;
        sceneVariant = newSceneVariant;
        if(sceneName != currentScene && sceneName != "")
        {
            Debug.Log($"Changing scene to {sceneName} at spawn point {spawnPointIndex}");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        else
        {
            var spawnPoins = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            var player = GameObject.FindGameObjectWithTag("Player").transform;
            ChangeSceneVariant();
            foreach (var sp in spawnPoins)
            {
                if (sp.index == spawnPointIndex)
                {
                    player.GetComponent<CharacterController>().enabled = false;
                    player.transform.position = sp.transform.position;
                    player.transform.rotation = sp.transform.rotation;
                    player.GetComponent<CharacterController>().enabled = true;
                    break;
                }
            }
        }
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        Debug.Log($"Spawn point {currentSpawnPointIndex}");
        DisableFinishedEncounters();
        sceneVariants = GameObject.FindGameObjectsWithTag("SceneVariant");
        print("found " + sceneVariants.Length + " scene variants");
        ChangeSceneVariant();
        SpawnPlayer(currentSpawnPointIndex);
    }

    void ChangeSceneVariant()
    {
        print("Changing to scene variant " + sceneVariant);
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
            print($"Added finished encounter {encounterID}");
        }
    }
    public void DisableFinishedEncounters()
    {
        var DisableEncounterObjects = FindObjectsByType<DisableEncounter>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var po in DisableEncounterObjects)
        {
            string poID = po.encounterID;
            if (finishedEncounters.Contains(poID))
            {
                print($"Disabling encounter object {poID}");
                po.Interact();
            }
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
        battleHUD = ui.transform.Find("BattleHUD");
        inventoryUI = ui.transform.Find("QuestHUD").GetComponent<RectTransform>();
        inventoryUI.gameObject.SetActive(false);
        var dialog = Object.Instantiate(Resources.Load<GameObject>("Dialog"));
        var spawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log("Found spawn points: " + spawnPoints.Length);
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
    }

    void UpdateBotActivation()
    {
        List<BotInput> allBots = FindObjectsByType<BotInput>(FindObjectsSortMode.None).ToList();
        // Create a list of bots with their priority score
        var rankedBots = allBots
            .Select(bot => new
            {
                bot,
                priority = CalculatePriority(bot)
            })
            .OrderByDescending(x => x.priority)   // Highest priority first
            .ToList();

        // Activate top N bots
        for (int i = 0; i < rankedBots.Count; i++)
        {
            bool shouldBeActive = i < maxActiveBots;
            rankedBots[i].bot.SetActiveBot(shouldBeActive);
        }
    }

    float CalculatePriority(BotInput bot)
    {
        var playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        // Lower distance = higher priority, so invert it
        float distanceScore = 1f / (Vector3.Distance(playerTransform.position, bot.transform.position) + 0.1f);

        // Active bots get a small boost
        float activeScore = bot.active ? 1f : 0f;

        // Stunned bots are lower priority
        float stunnedScore = bot.GetComponent<Character>().state == State.Stunned ? 0f : 1f;

        // Weighted sum
        float totalScore = (distanceScore * 3) +
                           (activeScore * 1) +
                           (stunnedScore * 2);
        return totalScore;
    }

    public void AddInventoryItem(string itemName, int quantity)
    {
        ShowMessage($"Found: {itemName}");
        foreach(var item in inventory)
        {
            if(item.itemName == itemName)
            {
                item.quantity += quantity;
                UpdateInventoryImages();
                return;
            }
        }
        UpdateInventoryImages();
        inventory.Add(new InventoryItem(itemName, quantity));
    }
    public bool ConsumeInventoryItem(string itemName, bool consume, int quantity)
    {
        foreach(var item in inventory)
        {
            if(item.itemName == itemName && item.quantity >= quantity)
            {
                if(consume){
                    item.quantity -= quantity;
                    if(item.quantity < 0) inventory.Remove(item);
                    ShowMessage($"Consumed: {itemName}");
                    UpdateInventoryImages();
                }
                return true;

            }
        }
        return false; //fail to find
    }
    public void UpdateInventoryImages()
    {
        var inventoryContainer = inventoryUI.Find("Items");
        foreach(Transform child in inventoryContainer){Destroy(child.gameObject);}
        foreach(var item in inventory)
        {
            var itemGO = Instantiate(Resources.Load<GameObject>("InventoryItem"), inventoryContainer);
            var itemText = itemGO.GetComponentInChildren<TMPro.TMP_Text>();
            if(itemText != null)
            {
                itemText.text = item.quantity.ToString();
            }
            var itemImage = itemGO.GetComponentInChildren<UnityEngine.UI.Image>();
            if(itemImage != null)
            {
                var sprite = Resources.Load<Sprite>($"Items/{item.itemName}");
                if(sprite != null)
                {
                    itemImage.sprite = sprite;
                }
            }
        }
    }

    public void AddQuest(string questName)
    {
        if(!quests.Contains(questName))
        {
            quests.Add(questName);
            ShowMessage($"New Quest: {questName} [Press I to view]");
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
        var questUI = inventoryUI.Find("Quests").GetComponent<TMP_Text>();
        questUI.text = "";
        foreach(var quest in quests)
        {
            questUI.text += "- " + quest + "\n";
        }
        
    }

}
