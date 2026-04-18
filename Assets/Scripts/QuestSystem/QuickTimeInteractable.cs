using UnityEngine;

public class QuickTimeInteractable : ChainedInteractable
{
    public override void Interact()
    {
        Instantiate(Resources.Load<GameObject>("QuickTimeEvent"));
        CallNext();
    }
}
