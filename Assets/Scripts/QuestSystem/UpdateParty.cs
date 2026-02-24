using UnityEngine;
using System.Collections.Generic;

public class UpdateParty : ChainedInteractable
{
    public bool overwrite = true;
    public string[] partyMembers;
    public override void Interact()
    {
        if(active){
            if(overwrite)
            {
                YourParty.instance.partyMembers.Clear();
            }
            foreach(var member in partyMembers)
            {
                if(YourParty.instance.partyMembers.Count < 3){
                    if(!YourParty.instance.partyMembers.Contains(member))
                        YourParty.instance.AddPartyMember(member);
                    else
                    {
                        GameManager.Instance.ShowMessage($"Already in your party");
                    }
                }
                else
                {
                    DialogBox d = FindFirstObjectByType<DialogBox>();
                    d.StartDialog(new List<Dialog>()
                    {
                        new Dialog()
                        {
                            cameraAngle = CameraAngle.behind,
                            text = "(Your party is already at maximum size)",
                        }
                    });
                }
            }
            foreach(ConditionalEncounter e in GameObject.FindObjectsByType<ConditionalEncounter>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                e.gameObject.SetActive(true);
                e.TryCheckConditions(); //Re-evaluate team based encounters
            }
            CallNext();  
        }
    }
}
