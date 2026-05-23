using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Choice : ChainedInteractable
{
    public string prompt;
    public string option1;
    public string option2;
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    public override void Interact()
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
        d.ShowChoiceButtons(option1, option2);
        d.OnDialogFinished += OnChoiceMade;
    }

    public void OnChoiceMade()
    {
        DialogBox d = FindFirstObjectByType<DialogBox>();
        if(d.choice == "Yes")
        {
            CallNext();
        }
        else
        {
            Fail();
        }
        d.OnDialogFinished -= OnChoiceMade;
    }
}



