using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Journal : ChainedInteractable
{
    public List<string> dialog = new List<string>();
    TMP_Text journalText;
    bool active2 = false;
    float charTimer = 0.1f;

    public override void Interact()
    {
        journalText = GameObject.Find("MainCanvas/OtherHUD/Journal").GetComponent<TMP_Text>();
        GameManager.Instance.SetGameplayState(GameplayState.Dialog);
        active2 = true;
    }

    void Update()
    {
        if (active2)
        {
            if(journalText.text.Length < dialog[0].Length)
            {
                if(charTimer <= 0f)
                {
                    journalText.text += dialog[0][journalText.text.Length];
                    charTimer = 0.03f;
                }
                if(Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse0))
                {
                    journalText.text = dialog[0];
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Mouse0))
                {
                    dialog.RemoveAt(0);
                    journalText.text = "";
                    if (dialog.Count == 0) 
                    {
                        active2 = false;
                        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
                        CallNext();
                    }
                }
            }
            if(charTimer > 0f) charTimer -= Time.deltaTime;
        }
    }
}
