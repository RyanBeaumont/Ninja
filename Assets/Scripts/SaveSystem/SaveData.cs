using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string sceneName;
    public int sceneVariant;
    public int spawnPoint;
    public float playTime;
    public float gold;
    public List<string> playersInParty;
    public List<SavePartyMember> reserve;
    public List<string> items = new List<string>();
    public List<int> itemQuantities = new List<int>();
    public List<string> finishedEncounters = new List<string>();
    public List<string> quests = new List<string>();

    public string saveFileName = "savefile_1";

    public SaveData()
    {
        playersInParty = YourParty.instance.partyMembers;
        reserve = new List<SavePartyMember>();
        foreach (var member in YourParty.instance.reserve)
        {
            SavePartyMember saveMember = YourParty.instance.ConvertToSavePartyMember(member);
            reserve.Add(saveMember);
        }
        foreach(InventoryItem item in GameManager.Instance.inventory)
        {
            items.Add(item.itemName);
            itemQuantities.Add(item.quantity);
        }
        finishedEncounters = GameManager.Instance.finishedEncounters;
        quests = GameManager.Instance.quests;
        playTime = GameManager.Instance.playTime;
        gold = YourParty.instance.gold;
        sceneVariant = GameManager.Instance.sceneVariant;
        spawnPoint = GameManager.Instance.currentSpawnPointIndex;
        sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        saveFileName = YourParty.instance.currentSaveFileName;
        Debug.Log($"Saved {reserve.Count} reserve members.");
    }

}

[System.Serializable]
public class SavePartyMember
{
    public string memberName;
    public int level;
    public int xp;
    public float hpPercentage = 1f;
    public List<string> deck;
}

