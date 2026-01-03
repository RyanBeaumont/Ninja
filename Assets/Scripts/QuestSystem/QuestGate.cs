using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestGate : ChainedInteractable
{
    public string questName;
    public bool consume = true;
    public float xpReward = 0;
    public float goldReward = 0;
    public bool completeEvenIfMissing = true;
    public List<Dialog> incompleteDialog = new List<Dialog>();

    public override void Interact()
    {
        DialogBox d = FindFirstObjectByType<DialogBox>();
        if(GameManager.Instance.ConsumeQuest(questName, consume) || completeEvenIfMissing)
        {
            Debug.Log("QuestGate: Quest " + questName + " completed.");
            d.StartDialog(YourParty.instance.LevelUp((int)xpReward,(int)goldReward));
            d.OnDialogFinished += OnEnd;
            return;
        }
        else
        {
            
            d.StartDialog(incompleteDialog);
        }
    }

    public void OnEnd()
    {
        DialogBox d = FindFirstObjectByType<DialogBox>();
        d.OnDialogFinished -= OnEnd;
        CallNext();
    }
}



