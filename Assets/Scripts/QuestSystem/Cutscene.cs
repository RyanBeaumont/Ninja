using UnityEngine;
using System.Collections;

public class Cutscene : ChainedInteractable
{
    public Transform model;
    public Transform targetPosition;
    public float moveTime = 5f;
    public string pose = "Running";
    public bool waitForEnd = true;
    public override void Interact()
    {
        if (active)
        {
            GameManager.Instance.SetGameplayState(GameplayState.Dialog);
            var anim = model.GetComponent<Animator>();
            if(pose != "" && model != null) anim.Play(pose);
            if(targetPosition != null && model != null) StartCoroutine(MoveModel());
            //if(!waitForEnd){GameManager.Instance.SetGameplayState(GameplayState.FreeMovement); CallNext();}
        }
    }

    IEnumerator MoveModel()
    {
        //Move model to targetPosition over moveTime seconds
        float elapsedTime = 0f;
        Vector3 startingPos = model.position;
        Quaternion startingRot = model.rotation;
        while (elapsedTime < moveTime)
        {
            float t = (elapsedTime / moveTime);
            model.position = Vector3.Lerp(startingPos, targetPosition.position, t);
            // Smoothly rotate to look at the target position (rather than matching the target's rotation)
            Vector3 currentPos = model.position;
            Vector3 lookDir = targetPosition.position - currentPos;
            Quaternion desiredRot = startingRot;
            if (lookDir.sqrMagnitude > 0.0001f)
            {
                desiredRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            }
            model.rotation = Quaternion.Slerp(startingRot, desiredRot, Mathf.Min(elapsedTime/0.3f,1f));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        model.position = targetPosition.position;
        // Final rotation: try to look at the target position from the final position. If that's degenerate,
        // fall back to the target's rotation to avoid zero-length look vectors.
        Vector3 finalLookDir = targetPosition.position - model.position;
        if (finalLookDir.sqrMagnitude > 0.0001f)
        {
            model.rotation = Quaternion.LookRotation(finalLookDir.normalized, Vector3.up);
        }
        else
        {
            model.rotation = targetPosition.rotation;
        }
        var anim = model.GetComponent<Animator>();
        anim.Play("Idle");
        if(waitForEnd){
            GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
            CallNext();
        }
       
    }
}
