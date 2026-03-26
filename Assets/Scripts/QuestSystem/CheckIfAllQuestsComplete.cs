using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheckIfAllQuestsComplete : ChainedInteractable
{
    public string[] questsToComplete;

    public override void Interact()
    {
        foreach(string quest in questsToComplete)
            if (GameManager.Instance.quests.Contains(quest))
            {
                Fail();
                return;
            }
        CallNext();
    }
    
}
