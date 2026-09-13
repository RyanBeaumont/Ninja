using UnityEngine;

public class QuitInteractable : ChainedInteractable
{
    public override void Interact()
    {
        Application.Quit();
    }
}