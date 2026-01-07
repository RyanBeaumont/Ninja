using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
}
public class YourParty : MonoBehaviour
{
    
    public static YourParty instance;
    public List<PartyMember> reserve;
    public List<string> partyMembers;
    public float hpPerLevel = 10f;
    public float speedPerLevel = 1f;
    public float attackPerLevel = 2f;
    public float defensePerLevel = 2f;
    public float psychicPerLevel = 2f;
    public float gold;
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

    void Start()
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


    public void StartEncounter(List<GameObject> enemyPrefabs, Transform position)
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
        var spacing = 2f;
        
        // Add party members as combatants
        for(int i=0; i< partyMembers.Count; i++)
        {
            var partyMember = GetPartyMember(partyMembers[i]);
            var combatantObject = Instantiate(Resources.Load<GameObject>("PlayerCombatant"), playerSpawn);

            //give cards
            var doubleDeck = new List<Card>(partyMember.deck);
            doubleDeck.AddRange(partyMember.deck);
            combatantObject.GetComponent<PlayerCombatant>().deck = doubleDeck;
            combatantObject.GetComponent<PlayerCombatant>().DrawCards(4);

            //spread out combatants centered around spawn point
            combatantObject.transform.localPosition = new Vector3((-0.5f*spacing*partyMembers.Count) + (i * spacing), 0f, 0f);
            var combatant = combatantObject.GetComponent<Combatant>();
            battleManager.AddCombatant(combatant);

            combatant.combatantName = partyMember.memberName;
            combatant.maxHp = hpPerLevel * partyMember.level + 100f;
            combatant.hp = combatant.maxHp * partyMember.hpPercentage ;
            print($"{combatant.combatantName} HP: {combatant.hp}/{combatant.maxHp} HP PERCENT {partyMember.hpPercentage}");
            var levelBonus = (float)partyMember.level; levelBonus += partyMember.mainClass == CardClass.Ninja ? 2f : 0f; levelBonus += partyMember.subClass == CardClass.Ninja ? 1f : 0f;
            combatant.speed = speedPerLevel * levelBonus + 10f;
            levelBonus = (float)partyMember.level; levelBonus += partyMember.mainClass == CardClass.Warrior ? 2f : 0f; levelBonus += partyMember.subClass == CardClass.Warrior ? 1f : 0f;
            combatant.attack = attackPerLevel * levelBonus + 10f;
            levelBonus = (float)partyMember.level; levelBonus += partyMember.mainClass == CardClass.Grappler ? 2f : 0f; levelBonus += partyMember.subClass == CardClass.Grappler ? 1f : 0f;
            combatant.defense = defensePerLevel * levelBonus + 10f;
            levelBonus = (float)partyMember.level; levelBonus += partyMember.mainClass == CardClass.Psychic ? 2f : 0f; levelBonus += partyMember.subClass == CardClass.Psychic ? 1f : 0f;
            combatant.psychic = psychicPerLevel * levelBonus + 10f;
            combatant.level = partyMember.level;
            var model = Instantiate(Resources.Load<GameObject>($"Characters/{partyMember.modelName}"), combatantObject.transform);

            var healthbar = Instantiate(Resources.Load<GameObject>("Health"), combatantObject.transform);

            combatant.enabled = true;    

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
            var combatant = combatantObject.GetComponent<Combatant>();
            var healthbar = Instantiate(Resources.Load<GameObject>("Health"), combatantObject.transform);
            battleManager.AddCombatant(combatant);
            GameManager.Instance.ShowMessage($"{combatant.combatantName} appears!");
            var effect = Instantiate(Resources.Load<GameObject>("Particles/Encounter"), combatantObject.transform);
            combatant.enabled = true;
        }

        

        BattleManager.Instance.StartBattle();

        
    }

    public void AddPartyMember(string memberName)
    {
        if(!partyMembers.Contains(memberName))
        {
            partyMembers.Add(memberName);
        }
    }

    public void RemovePartyMember(string memberName)
    {
        if(partyMembers.Contains(memberName))
        {
            partyMembers.Remove(memberName);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            var dialog = LevelUp(50,50);
            GameManager.Instance.AddInventoryItem("Coke", 1);
            var dialogBox = FindFirstObjectByType<DialogBox>();
            dialogBox.StartDialog(dialog);
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
            }
        }
        return dialog;
    
    }

}
