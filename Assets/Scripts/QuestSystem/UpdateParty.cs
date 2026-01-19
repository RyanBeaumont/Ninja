using UnityEngine;

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
                YourParty.instance.AddPartyMember(member);
            }
            CallNext();  
        }
    }
}
