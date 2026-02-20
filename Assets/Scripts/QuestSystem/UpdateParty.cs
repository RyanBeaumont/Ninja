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
                if(!YourParty.instance.partyMembers.Contains(member))
                    YourParty.instance.AddPartyMember(member);
                else
                {
                    GameManager.Instance.ShowMessage($"Already in your party");
                }
            }
            CallNext();  
        }
    }
}
