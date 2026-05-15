using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable] public class PartyMember
{
    public string memberName;
    public string modelName;
    public int level;
    public int xp;
    public float hpPercentage = 1f;
    public CardClass mainClass;
    public CardClass subClass;
    public List<Card> deck;
    public List<InventoryItem> equipment;
    public bool alive = true;
}
public class YourParty : MonoBehaviour
{
    
    public static YourParty instance;
    public List<PartyMember> reserve;
    public List<string> partyMembers;
    public FloatValue gameDifficulty;

    public bool devTools = false;
    public string currentSaveFileName = "savefile_1";
    public float gold;
    public float spacing = 1f;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void BuildStartingDeck()
    {
        //Give starting decks
        foreach(var member in reserve)
        {
            member.deck = CardDatabase.Instance.BuildDeckByClass(member.mainClass, member.subClass, member.level);
        }
    }

    public PartyMember GetPartyMember(string memberName)
    {
        return reserve.Find(member => member.memberName == memberName);
    }

    public SavePartyMember ConvertToSavePartyMember(PartyMember member)
    {
        SavePartyMember saveMember = new SavePartyMember();
        saveMember.memberName = member.memberName;
        saveMember.level = member.level;
        saveMember.xp = member.xp;
        saveMember.hpPercentage = member.hpPercentage;
        saveMember.deck = new List<string>();
        foreach(Equipment equipment in member.equipment)
        {
            if(equipment.itemName != null){
                saveMember.equipment.Add(equipment.itemName);
            }
        }
        saveMember.alive = member.alive;
        foreach (var card in member.deck)
        {
            saveMember.deck.Add(card.cardName); // Assuming Card has a cardName property
        }
        return saveMember;
    }

    public PartyMember ConvertFromSavePartyMember(SavePartyMember saveMember)
    {
        PartyMember member = GetPartyMember(saveMember.memberName);
        if (member != null)
        {
            member.level = saveMember.level;
            member.xp = saveMember.xp;
            member.hpPercentage = saveMember.hpPercentage;
            foreach(string equipmentName in saveMember.equipment)
            {
                var equipment = GameManager.Instance.GetInventoryItemByName(equipmentName);
                member.equipment.Add(equipment);
            }
            member.deck = new List<Card>();
            member.alive = saveMember.alive;
            foreach (var cardName in saveMember.deck)
            {
                Card card = CardDatabase.Instance.GetCardByName(cardName);
                if (card != null)
                {
                    member.deck.Add(card);
                }
            }
        }
        return member;
    }

    public void LoadLastSave()
    {
        var data = SaveSystem.LoadGame(currentSaveFileName);
        if(data != null)
        {
            LoadGame(data);
        }
        else
        {
            Debug.LogError("No save data found to load.");
            SceneManager.LoadScene("TitleScene");
        }
    }

    public void LoadGame(SaveData data)
    {
        partyMembers = data.playersInParty;
        foreach(var saveMember in data.reserve)
        {
            var member = ConvertFromSavePartyMember(saveMember);
        }
        GameManager.Instance.inventory.Clear();
        for(int i=0; i< data.items.Count; i++)
        {
            GameManager.Instance.AddInventoryItem(data.items[i], data.itemQuantities[i]);
        }
        gold = data.gold;
        GameManager.Instance.finishedEncounters = data.finishedEncounters;
        foreach(var quest in data.quests)
        {
            GameManager.Instance.AddQuest(quest);
        }
        GameManager.Instance.playTime = data.playTime;
        GameManager.Instance.ChangeScene(data.sceneName, data.spawnPoint, data.sceneVariant);
        currentSaveFileName = data.saveFileName;
    }

    public void GetStats(PartyMember partyMember, out float attack, out float maxHp, out float speed, out float psychic)
    {
        var multiplier = 2f; if(partyMember.subClass == CardClass.Ninja) multiplier = 2.5f; if(partyMember.mainClass == CardClass.Ninja) multiplier = 3f;
        speed = partyMember.level * multiplier + 10f;
        foreach(Equipment e in partyMember.equipment)
            foreach(StatusEffect se in e.statusEffects)
                if(se.stat == "SPD") speed += se.amount;
        multiplier = 1f; if(partyMember.subClass == CardClass.Warrior) multiplier = 1.25f; if(partyMember.mainClass == CardClass.Warrior) multiplier = 1.5f;
        attack = partyMember.level * multiplier;
        foreach(Equipment e in partyMember.equipment)
            foreach(StatusEffect se in e.statusEffects)
                if(se.stat == "ATK") attack += se.amount;
        multiplier = 20f; if(partyMember.subClass == CardClass.Grappler) multiplier = 25f; if(partyMember.mainClass == CardClass.Grappler) multiplier = 30f;
        maxHp = partyMember.level * multiplier + 50f;
        foreach(Equipment e in partyMember.equipment)
            foreach(StatusEffect se in e.statusEffects)
                if(se.stat == "MAXHP") {
                    if(se.additive == true)
                        maxHp += se.amount;
                    else
                        maxHp *= se.amount;
                }
        multiplier = 1.5f; if(partyMember.subClass == CardClass.Psychic) multiplier = 1.75f; if(partyMember.mainClass == CardClass.Psychic) multiplier = 2f;
        psychic = partyMember.level * multiplier + 15f;
        foreach(Equipment e in partyMember.equipment)
            foreach(StatusEffect se in e.statusEffects)
                if(se.stat == "PSY") psychic += se.amount;
    }


    public void StartEncounter(List<GameObject> enemyPrefabs, Transform position, GameObject newplayer)
    {
        //Find closest battle area
         var battleAreas = GameObject.FindGameObjectsWithTag("BattleArea");
            //find nearest one
            float closestDistance = Mathf.Infinity;
            Transform closestArea = null;
            foreach(var area in battleAreas)
            {
                float distance = Vector3.Distance(position.position, area.transform.position);
                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    closestArea = area.transform;
                }
            }
        var BattleSetup = Instantiate(Resources.Load<GameObject>("BattleSetup"), closestArea.position, closestArea.rotation);
        BattleSetup.name = "BattleSetup";
        var battleManager = BattleSetup.GetComponent<BattleManager>();
        var playerSpawn = BattleSetup.transform.Find("PlayerSpawn");
        var enemySpawn = BattleSetup.transform.Find("EnemySpawn");
        
        
        // Add party members as combatants
        for(int i=0; i< partyMembers.Count; i++)
        {
            var partyMember = GetPartyMember(partyMembers[i]);
            var combatantObject = Instantiate(Resources.Load<GameObject>("PlayerCombatant"), playerSpawn);
            var model = Instantiate(Resources.Load<GameObject>($"Characters/{partyMember.modelName}"), combatantObject.transform);

            var healthbar = Instantiate(Resources.Load<GameObject>("Health"), combatantObject.transform);

            //give cards
            var doubleDeck = new List<Card>(partyMember.deck);
            doubleDeck.AddRange(partyMember.deck);
            combatantObject.GetComponent<PlayerCombatant>().deck = doubleDeck;
            combatantObject.GetComponent<PlayerCombatant>().ShuffleDeck();
            combatantObject.GetComponent<PlayerCombatant>().DrawCards(4);

            //spread out combatants centered around spawn point
            combatantObject.transform.localPosition = new Vector3((-0.5f*spacing*partyMembers.Count) + (i * spacing), 0f, 0f);
            var combatant = combatantObject.GetComponent<Combatant>();
            battleManager.AddCombatant(combatant);

            combatant.combatantName = partyMember.memberName;

            //print($"{combatant.combatantName} HP: {combatant.hp}/{combatant.maxHp} HP PERCENT {partyMember.hpPercentage}");
            GetStats(partyMember, out var attack, out var maxHp, out var speed, out var psychic);
            //check for equipment with other stats
            foreach(Equipment e in partyMember.equipment)
            {
                foreach(StatusEffect s in e.statusEffects){
                    
                    if(s.stat != "ATK" && s.stat != "DEF" && s.stat != "MAXHP" && s.stat != "SPD" && s.stat != "PSY")
                    {
                        combatant.statusEffects.Add(s);
                    }
                }
            }
                
            combatant.attack = attack; combatant.maxHp = maxHp; combatant.speed = speed; combatant.psychic = psychic;
            combatant.hp = combatant.maxHp * partyMember.hpPercentage ;
            combatant.maxMp = combatant.psychic * 4;
            combatant.level = partyMember.level;
            combatant.defense = 1f;
            if(gameDifficulty.value == 2) combatant.defense = 0.8f;
            if(gameDifficulty.value == 0) combatant.defense = 1.2f;
            
            
            combatant.enabled = true;
            if(partyMember.alive == false){combatant.alive = false; combatant.PlayAnimation("Knockdown");}
            combatant.GetComponentInChildren<Animator>().enabled = true;    

        }

        battleManager.enabled = true;

        // Add enemies as combatants
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
             var enemyPrefab = enemyPrefabs[i];
            var combatantObject = Instantiate(enemyPrefab, enemySpawn);
            //spread out combatants centered around spawn point
            combatantObject.transform.localPosition = new Vector3((-0.5f * spacing * enemyPrefabs.Count) + (i * spacing), 0f, 0f);
            combatantObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var combatant = combatantObject.GetComponent<EnemyCombatant>();
            if (combatant.boss)
            {
                var menu = FindFirstObjectByType<Menu>();
                if(menu != null){menu.bossHP.gameObject.SetActive(true);
                    menu.bossHP.GetComponent<Healthbar>().combatant = combatant;
                }
        
            }else{Instantiate(Resources.Load<GameObject>("Health"), combatantObject.transform);}
            battleManager.AddCombatant(combatant);
            GameManager.Instance.ShowMessage($"{combatant.combatantName} appears!");
            var effect = Instantiate(Resources.Load<GameObject>("Particles/Encounter"), combatantObject.transform);
            combatant.enabled = true;
        }

        

        BattleManager.Instance.StartBattle(newplayer);

        
    }

    public void AddPartyMember(string memberName)
    {
        if(!partyMembers.Contains(memberName))
        {
            partyMembers.Add(memberName);
            UpdateLeader();
        }
    }

    public void RemovePartyMember(string memberName)
    {
        if(partyMembers.Contains(memberName))
        {
            partyMembers.Remove(memberName);
        }
    }

     public void UpdateLeader()
    {
        var character = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<Character>();
        if(character == null) {print("Could not find player character"); return;}
        if(partyMembers.Count > 0)
        {
            var leader = partyMembers[0];
            var modelName = GetPartyMember(leader).modelName;
            StartCoroutine(character.ChangeModel(modelName));
            var menu = FindFirstObjectByType<Menu>();
            if(menu != null)menu.UpdateParty();
        }
    }

    void Update()
    {
        if(devTools){
        if (Input.GetKeyDown(KeyCode.L))
        {
            var dialog = LevelUp(150,150);
            GameManager.Instance.AddInventoryItem("Coke", 1);
            GameManager.Instance.AddInventoryItem("Bang", 1);
            var dialogBox = FindFirstObjectByType<DialogBox>();
            dialogBox.StartDialog(dialog);
        }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                GameManager.Instance.StartSceneTransition(SceneManager.GetActiveScene().name,GameManager.Instance.currentSpawnPointIndex,GameManager.Instance.sceneVariant + 1,null);
            }

        if(Input.GetKeyDown(KeyCode.K))
        {
            SaveSystem.SaveGame(currentSaveFileName);
            GameManager.Instance.ShowMessage("Game Saved!");
        }
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            devTools = !devTools;
            GameManager.Instance.ShowMessage($"Developer Cheat Codes: {devTools}");
        }
    }

    
    public void HealParty()
    {
        foreach(PartyMember m in reserve){
            m.hpPercentage = 1f;
            m.alive = true;
        }
    }

    public List<Dialog> LevelUp(int xpAmount, int goldAmount)
    {
        var dialog = new List<Dialog>();
        gold += goldAmount;
        dialog.Add(new Dialog()
        {
            name = "",
            text = $"Your party gained {xpAmount} XP and {goldAmount} Gold!",
            cameraAngle = CameraAngle.standard,
            face = "",
            pose = "",
            character = null,
        });
        foreach(var player in partyMembers)
        {
            var partyMember = GetPartyMember(player);
            partyMember.xp += xpAmount;
            //Level up if xp exceeds threshold
            int xpThreshold = 100 + partyMember.level * 10;
            bool levelUp = false;
            while(partyMember.xp >= xpThreshold)
            {
                partyMember.xp -= xpThreshold;
                partyMember.level += 1;
                xpThreshold = 100 + partyMember.level * 10;
                levelUp = true;
            }

            if(levelUp)
            {
                dialog.Add(new Dialog()
                {
                    name = player,
                    text = $"{player} leveled up to level {partyMember.level}!",
                    cameraAngle = CameraAngle.standard,
                    face = "Happy",
                    pose = "ArmsCrossed",
                    character = null,
                });

                //Get new cards for level up
                var newCards = CardDatabase.Instance.GetNewCardsForLevel(partyMember.mainClass, partyMember.subClass, partyMember.level);
                foreach(var card in newCards)
                {
                    var text = $"{player} learned: {card.cardName}!";
                    if(partyMember.deck.Count < CardDatabase.Instance.deckMax){
                        partyMember.deck.Add(card);
                        text += " (Deck maximum reached - You can swap out cards in the [ESC] menu)";
                    }

                    dialog.Add(new Dialog()
                    {
                        name = player,
                        text = $"{player} learned: {card.cardName}!",
                        cameraAngle = CameraAngle.standard,
                        face = "Happy",
                        pose = "ArmsCrossed",
                        character = null,
                    });
                }
            }
        }
        return dialog;
    
    }

}