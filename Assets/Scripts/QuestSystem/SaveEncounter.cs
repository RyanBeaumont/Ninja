using UnityEngine;
using System.Collections.Generic;

public class SaveEncounter : ChainedInteractable
{
    public override void Interact()
    {
        if (active)
        {
            if (GetComponentInChildren<SpawnPoint>())
            {
                GameManager.Instance.SetSpawnPoint(GetComponentInChildren<SpawnPoint>().index);
            }
            SaveSystem.SaveGame(YourParty.instance.currentSaveFileName);
            GameManager.Instance.ShowMessage("Game Saved!");
            CallNext();
        }
    }
}
