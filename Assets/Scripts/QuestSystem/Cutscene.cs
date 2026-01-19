using UnityEngine;
using System.Collections;
using Unity.Cinemachine;

[System.Serializable]
public class Waypoint
{
    public Transform waypointTransform;
    public string animation;
    public float duration;
    public string sound = "";
}

public class Cutscene : ChainedInteractable
{
    public Transform model;
    public Waypoint[] waypoints;
    public bool waitForEnd = true;
    public Transform cameraSource;
    GameObject cameraRig;
    CinemachineCamera cutsceneCamera;
    Animator cameraAnimator;
    public override void Interact()
    {
        if (active)
        {
            GameManager.Instance.SetGameplayState(GameplayState.Dialog);
            if(model == null) model = GameObject.FindGameObjectWithTag("Player").transform;
            var anim = model.GetComponent<Animator>();
            if(waypoints.Length > 0 && model != null) StartCoroutine(MoveModel());
            //if(!waitForEnd){GameManager.Instance.SetGameplayState(GameplayState.FreeMovement); CallNext();}

            if(cameraSource == null)
            {
                cameraSource = GameObject.FindGameObjectWithTag("Player").transform;
            }

            if(cameraSource != null)
            {
                cameraRig = Instantiate(Resources.Load<GameObject>("CameraRig"));
                cameraAnimator = cameraRig.GetComponent<Animator>();
                cutsceneCamera = cameraRig.GetComponentInChildren<CinemachineCamera>();
                cutsceneCamera.Priority = 10;
                cameraRig.transform.parent = cameraSource;
                cameraRig.transform.localRotation = Quaternion.identity;
                cameraRig.transform.localPosition = new Vector3(0f,0f,0f);
            }
        }
    }

    IEnumerator MoveModel()
{
    var anim = model.GetComponent<Animator>();

    for (int i = 0; i < waypoints.Length; i++)
    {
        Waypoint wp = waypoints[i];
        Transform target = wp.waypointTransform;
        if(wp.sound != "")
        {
            AudioManager.Instance.PlaySoundEffect(wp.sound);
        }

        // Play movement animation for this waypoint
        if (!string.IsNullOrEmpty(wp.animation))
        {
            anim.Play(wp.animation, 0, 0f);
        }

        Vector3 startPos = model.position;
        Quaternion startRot = model.rotation;

        float elapsed = 0f;

        while (elapsed < wp.duration)
        {
            float t = elapsed / wp.duration;

            // Position
            model.position = Vector3.Lerp(startPos, target.position, t);

            // Face movement direction

            Vector3 moveDir = (target.position - model.position);
            if (moveDir.sqrMagnitude > 0.0001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(moveDir.normalized, Vector3.up);
                model.rotation = Quaternion.Slerp(startRot, lookRot, Mathf.Clamp01(elapsed / 0.25f));
            }

            // Camera logic (unchanged)
            if (cutsceneCamera != null)
            {
                cameraAnimator.Play("Camera_Behind");
                cutsceneCamera.transform.LookAt(model);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to final waypoint position
        model.position = target.position;

        // If this is the LAST waypoint, smoothly align to its rotation
        if (i == waypoints.Length - 1)
        {
            yield return StartCoroutine(AlignRotation(target.rotation));
        }
    }

    // End pose
    anim.Play("Idle");

    if (waitForEnd)
    {
        if (cameraRig != null) Destroy(cameraRig);
        GameManager.Instance.SetGameplayState(GameplayState.FreeMovement);
        CallNext();
    }
}

IEnumerator AlignRotation(Quaternion targetRotation)
{
    Quaternion startRot = model.rotation;
    float duration = 0.4f;
    float t = 0f;

    while (t < duration)
    {
        model.rotation = Quaternion.Slerp(startRot, targetRotation, t / duration);
        t += Time.deltaTime;
        yield return null;
    }

    model.rotation = targetRotation;
}


}
