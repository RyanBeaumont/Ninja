using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class SaveEncounter : ChainedInteractable
{
    bool uiActive = false;
    GameObject ui;
    public override void Interact()
    {
        if (active)
        {
            ui = Instantiate(Resources.Load<GameObject>("Saves"));
            ui.transform.Find("Saves/Title").GetComponent<TMP_Text>().text = "Save Your Game";
            Transform content = ui.transform.Find("Saves/Viewport/Content");
            foreach (var save in SaveSystem.GetAllSaves())
            {
                GameObject loadButton = Instantiate(Resources.Load<GameObject>("LoadButton"), content);
                loadButton.GetComponent<LoadGameButton>().SetSaveData(save, true);
                loadButton.GetComponent<Button>().onClick.AddListener(OnSaveFinished); //Button already auto-sets save file name
            }
            GameObject newSave = Instantiate(Resources.Load<GameObject>("NewSave"), content);
            newSave.GetComponent<Button>().onClick.AddListener(NewSave);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
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
        OnSaveFinished();
    }

    public void OnSaveFinished()
    {
        if(ui != null) Destroy(ui);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        if (GetComponentInChildren<SpawnPoint>())
            {
                GameManager.Instance.SetSpawnPoint(GetComponentInChildren<SpawnPoint>().index);
            }
        YourParty.instance.HealParty();
        SaveSystem.SaveGame(YourParty.instance.currentSaveFileName);
        GameManager.Instance.ShowMessage($"Party Healed and Game Saved to {YourParty.instance.currentSaveFileName}");
        CallNext();
    }
}
