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
            var player = GameObject.FindGameObjectWithTag("Player");
            player.GetComponent<Animator>().Play("ArmsCrossed");
            yield return StartCoroutine(GameManager.Instance.Fade(toBlack, cameraTarget));
            CallNext();
        }
    }

    
}
