using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemGate : ChainedInteractable
{
    public string prompt;
    public string itemName;
    public int itemQuantity = 1;
    public bool consume = true;
    Animator animator;
    public List<Dialog> incompleteDialog = new List<Dialog>();

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    public override void Interact()
    {
        if(GameManager.Instance.ConsumeInventoryItem(itemName, false, itemQuantity))
        {
            DialogBox d = FindFirstObjectByType<DialogBox>();
            Dialog choiceDialog = new Dialog()
            {
                text = prompt,
                name = "",
                character = transform,
                pose = "",
                cameraAngle = CameraAngle.behind,
                face = ""
            };
            d.StartDialog(new List<Dialog>() { choiceDialog });
            d.ShowChoiceButtons();
            d.OnDialogFinished += OnChoiceMade;
        }
        else
        {
            DialogBox d = FindFirstObjectByType<DialogBox>();
            d.StartDialog(incompleteDialog);
        }
    }

    public void OnChoiceMade()
    {
        DialogBox d = FindFirstObjectByType<DialogBox>();
        if(d.choice == "Yes")
        {
            print("Yes");
            bool success = GameManager.Instance.ConsumeInventoryItem(itemName, consume, itemQuantity);
            if(success)
            {
                CallNext();
            }
            else
            {
                d.StartDialog(incompleteDialog);
            }
        }
        else
        {
            print("No");
        }
        d.OnDialogFinished -= OnChoiceMade;
    }
}



