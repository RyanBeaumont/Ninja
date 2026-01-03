using UnityEngine;

public class DisableEncounter : ChainedInteractable
{
    public bool hideObject = true;
    public GameObject[] additionalObjects;
    [SerializeField] string endingPose = "";
    [SerializeField] string endingFace = "";
    public override void Interact()
    {
        print("Disabling encounter object via DisableEncounter script");
        GameManager.Instance.AddEncounter($"{gameObject.scene.name}_{transform.position}");
        
        if(hideObject){
            Destroy(gameObject);
            foreach(GameObject obj in additionalObjects){
                Destroy(obj);
            }
        } else {
          
            var defaultPose = GetComponentInChildren<DefaultPose>();
            if(defaultPose != null){
                defaultPose.pose = endingPose;
                defaultPose.face = endingFace;
                defaultPose.PlayDefault();
            }
            foreach(GameObject obj in additionalObjects){
                defaultPose = obj.GetComponentInChildren<DefaultPose>();
                if(defaultPose != null){
                    defaultPose.pose = endingPose;
                    defaultPose.face = endingFace;
                    defaultPose.PlayDefault();
                }
            }
              foreach(ChainedInteractable ci in transform.GetComponentsInChildren<ChainedInteractable>())
                {
                    if (ci != this){ci.active = false;}
                }
        }
    }
}
