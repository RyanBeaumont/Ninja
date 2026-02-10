using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

public class FadeToBlack : ChainedInteractable
{
    public bool toBlack = true;
    public Transform cameraTarget;
    GameObject cameraRig;
    public override void Interact()
    {
        StartCoroutine(InteractCoroutine());
    }

    IEnumerator InteractCoroutine()
    {
        if (active)
        {
            GameManager.Instance.SetGameplayState(GameplayState.Dialog);
            var player = GameObject.FindGameObjectWithTag("Player");
            player.GetComponentInChildren<Animator>().Play("ArmsCrossed");
            yield return StartCoroutine(GameManager.Instance.Fade(toBlack, cameraTarget));
            GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
            CallNext();
        }
    }

    
}
