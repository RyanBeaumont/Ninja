using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestGiver : ChainedInteractable
{
    public string questName;

    public override void Interact()
    {
        GameManager.Instance.AddQuest(questName);
        CallNext();
    }
}



