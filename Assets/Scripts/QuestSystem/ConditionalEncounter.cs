using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConditionalEncounter : ChainedInteractable
{
    public int levelGate = -1;
    public List<int> allowedSceneVariants = new List<int>();
    public string needsPartyMember = "";
    public string excludePartyMember = "";
    public bool hideObject = false;

    bool CheckConditions()
    {
        if(levelGate > -1)
        {
            int partyAvgLevel = 0;
            foreach(var memberName in YourParty.instance.partyMembers)
            {
                var member = YourParty.instance.GetPartyMember(memberName);
                partyAvgLevel += member.level;
            }
            partyAvgLevel /= YourParty.instance.partyMembers.Count;
            if(partyAvgLevel < levelGate) return false;
        }

        if(allowedSceneVariants.Count > 0 && !allowedSceneVariants.Contains(GameManager.Instance.sceneVariant)) return false;
        if(needsPartyMember != "" && !YourParty.instance.partyMembers.Contains(needsPartyMember)) return false;
        if(excludePartyMember != "" && YourParty.instance.partyMembers.Contains(excludePartyMember)) return false;
        return true;
    }

    public void TryCheckConditions()
    {
        if(CheckConditions() == false)
        {
            foreach(ChainedInteractable i in transform.GetComponents<ChainedInteractable>()) i.active = false;
            if(hideObject) gameObject.SetActive(false);
        }
    }
    public override void Interact()
    {
        if(CheckConditions()) CallNext();
    }
}
