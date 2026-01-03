using UnityEngine;

public class DefaultPose : MonoBehaviour
{
    public string pose;
    public string face;

    void Start()
    {
        PlayDefault();
    }

    public void PlayDefault()
    {
        var animator = GetComponent<Animator>();
        if(animator != null && pose != ""){
            animator.Play(pose);
        }
        var faceChanger = GetComponentInChildren<FaceChanger>();
        if(faceChanger != null && face != ""){
            faceChanger.ChangeFace(face);
        }
    }
}
