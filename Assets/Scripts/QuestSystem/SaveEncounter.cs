using UnityEngine;
using System.Collections.Generic;

public class SaveEncounter : ChainedInteractable
{
    public override void Interact()
    {
        if (active)
        {
            SaveSystem.SaveGame(YourParty.instance.currentSaveFileName);
            GameManager.Instance.ShowMessage("Game Saved!");
        }
    }
}
