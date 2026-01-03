using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class FadeToBlack : ChainedInteractable
{
    public bool toBlack = true;
    public override void Interact()
    {
        StartCoroutine(InteractCoroutine());
    }

    IEnumerator InteractCoroutine()
    {
        if (active)
        {
            GameManager.Instance.SetGameplayState(GameplayState.Dialog);
            yield return StartCoroutine(GameManager.Instance.Fade(toBlack));
            GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
            CallNext();
        }
    }

    
}
