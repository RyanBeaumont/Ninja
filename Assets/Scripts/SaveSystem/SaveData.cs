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

}

public static class SaveDataBuilder
{
    public static SaveData Build(string saveFileName)
    {
        SaveData data = new SaveData();
        data.playersInParty = YourParty.instance.partyMembers;
        data.reserve = new List<SavePartyMember>();
        foreach (var member in YourParty.instance.reserve)
        {
            SavePartyMember saveMember = YourParty.instance.ConvertToSavePartyMember(member);
            data.reserve.Add(saveMember);
        }
        foreach(InventoryItem item in GameManager.Instance.inventory)
        {
            data.items.Add(item.itemName);
            data.itemQuantities.Add(item.quantity);
        }
        data.finishedEncounters = GameManager.Instance.finishedEncounters;
        data.quests = GameManager.Instance.quests;
        data.playTime = GameManager.Instance.playTime;
        data.gold = YourParty.instance.gold;
        data.sceneVariant = GameManager.Instance.sceneVariant;
        data.spawnPoint = GameManager.Instance.currentSpawnPointIndex;
        data.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        saveFileName = YourParty.instance.currentSaveFileName;
        Debug.Log($"Saved {data.reserve.Count} reserve members.");

        return data;
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
    public List<InventoryItem> equipment;
}

