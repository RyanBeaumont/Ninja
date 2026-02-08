using UnityEngine;

public class StartingPose : ChainedInteractable
{
    public string pose;
    public string face;

    public override void Interact()
    {
       Invoke("PlayDefault", 0.2f);
       CallNext();
    }

    public void PlayDefault()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        var animator = player.GetComponent<Animator>();
        if(animator != null && pose != ""){
            animator.Play(pose);
        }
        var faceChanger = player.GetComponentInChildren<FaceChanger>();
        if(faceChanger != null && face != ""){
            faceChanger.ChangeFace(face);
        }
    }
}