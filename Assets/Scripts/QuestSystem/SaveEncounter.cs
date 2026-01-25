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
            YourParty.instance.HealParty();
            SaveSystem.SaveGame(YourParty.instance.currentSaveFileName);
            GameManager.Instance.ShowMessage("Party Healed and Game Saved!");
            CallNext();
        }
    }
}
