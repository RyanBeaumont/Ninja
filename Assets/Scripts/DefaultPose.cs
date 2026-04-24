using UnityEngine;

public class DefaultPose : MonoBehaviour
{
    public string pose;
    public string face;
    public string combatIdle;
    public GameObject combatWeapon;
    public Color color;

    void Start()
    {
        if(GetComponentInParent<Character>() == null)
        PlayDefault();
    }

    public void PlayDefault()
    {
        var animator = GetComponentInChildren<Animator>();
        if(animator != null && pose != ""){
            animator.Play(pose);
        }
        var faceChanger = GetComponentInChildren<FaceChanger>();
        if(faceChanger != null && face != ""){
            faceChanger.ChangeFace(face);
        }
    }
}
