using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class SaveEncounter : ChainedInteractable
{
    GameObject ui;
    public override void Interact()
{
    if (!active) return;
    GameManager.Instance.SetGameplayState(GameplayState.Dialog);
    ui = Instantiate(Resources.Load<GameObject>("Saves"));
    ui.transform.Find("Saves/Title").GetComponent<TMP_Text>().text = "Save Your Game";

    Transform content = ui.transform.Find("Saves/Viewport/Content");

    var saveSlots = new[] { "savefile_1", "savefile_2", "savefile_3" };
    var allSaves = SaveSystem.GetAllSaves();
    var saveMap = new Dictionary<string, SaveData>();
    foreach (var save in allSaves)
    {
        if (!string.IsNullOrEmpty(save.saveFileName))
            saveMap[save.saveFileName] = save;
    }

    foreach (var slotName in saveSlots)
    {
        GameObject loadButton = Instantiate(Resources.Load<GameObject>("LoadButton"), content);
        var buttonScript = loadButton.GetComponent<LoadGameButton>();

        if (saveMap.TryGetValue(slotName, out var saveData))
        {
            buttonScript.SetSaveData(saveData, true);
        }
        else
        {
            loadButton.GetComponentInChildren<TMP_Text>().text = $"Empty Slot";
        }

        string capturedSlot = slotName;
        var button = loadButton.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnSaveFinished(capturedSlot));
    }

    Time.timeScale = 0f;
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
}

    public void NewSave()
    {
        //If save is like ""savefile_1"; add one to the end until you find an empty one
        int saveIndex = 1;
        var allSaves = SaveSystem.GetAllSaves();
        while (allSaves.Exists(s => s.saveFileName == "savefile_" + saveIndex))
        {
            saveIndex++;
        }
        YourParty.instance.currentSaveFileName = "savefile_" + saveIndex;
        OnSaveFinished(YourParty.instance.currentSaveFileName);
    }

    public void OnSaveFinished(string saveFileName)
{
    print("Saving to " + saveFileName);
    if (ui != null) Destroy(ui);

    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
    Time.timeScale = 1f;

    if (GetComponentInChildren<SpawnPoint>())
    {
        GameManager.Instance.SetSpawnPoint(GetComponentInChildren<SpawnPoint>().index);
    }

    YourParty.instance.HealParty();
    YourParty.instance.currentSaveFileName = saveFileName;

    SaveSystem.SaveGame(saveFileName);
    GameManager.Instance.ShowMessage($"Party Healed and Game Saved");
    GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
    CallNext();
}
}
