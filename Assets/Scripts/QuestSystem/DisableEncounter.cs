using UnityEngine;

public class DisableEncounter : ChainedInteractable
{
    public bool hideObject = true;
    public GameObject[] additionalObjects;
    [SerializeField] string endingPose = "";
    [SerializeField] string endingFace = "";
    Vector3 originalPosition;

    protected override void Awake()
    {
        originalPosition = transform.position;
        base.Awake();
    }
    public override void Interact()
    {
        GameManager.Instance.AddEncounter($"{gameObject.scene.name}_{originalPosition}");
        foreach(ChainedInteractable ci in transform.GetComponentsInChildren<ChainedInteractable>())
        {
            if (ci != this){ci.active = false;}
            if(ci is Cutscene cutscene)
            {
                cutscene.SkipToEndOfCutscene();
                Debug.Log($"Skipped cutscene {cutscene.name}");
            }
        }


        if(hideObject){
            Destroy(gameObject);
            foreach(GameObject obj in additionalObjects){
                Destroy(obj);
            }
        } else {
          
            var questIcon = GetComponentInChildren<QuestIcon>();
            if(questIcon != null) Destroy(questIcon.gameObject);
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
              
        }

        
    }
}
