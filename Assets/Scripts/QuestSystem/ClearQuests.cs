using UnityEngine;

public class ClearQuests : ChainedInteractable{
    
    public override void Interact()
    {
        GameManager.Instance.ClearQuests();
        CallNext();
    }

}
