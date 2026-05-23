using UnityEngine;

public class GoTo : ChainedInteractable
{
    public ChainedInteractable nextInteractable;
    public override void Interact()
    {
        if (nextInteractable != null)
        {
            nextInteractable.Interact();
        }
    }
}
