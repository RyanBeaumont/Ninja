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
    public float rotationSpeed = 720f; // degrees per second when rotating toward target
    public Waypoint[] waypoints;
    public bool waitForEnd = true;
    public Transform cameraSource;
    GameObject cameraRig;
    CinemachineCamera cutsceneCamera;
    Animator cameraAnimator;
    Transform originalParent;
    Vector3 originalPosition;
    Quaternion originalRotation;
    public override void Interact()
    {
        if (active)
        {
            GameManager.Instance.SetGameplayState(GameplayState.Dialog);
            var player = GameObject.FindGameObjectWithTag("Player").transform;
            if(model == null) model = player;
            player.GetComponentInChildren<Animator>().Play("ArmsCrossed");
            var anim = model.GetComponentInChildren<Animator>();
            if(waypoints.Length > 0 && model != null) StartCoroutine(MoveModel());

            if(!waitForEnd){ CallNext(); return;}; //Don't lock the camera on

            if(cameraSource == null)
            {
                cameraSource = GameObject.FindGameObjectWithTag("Player").transform;
                Debug.Log("Defaulting to player camera source");
            }

            if(cameraSource != null)
            {
                cameraRig = GameManager.Instance.GetCamera(out cameraAnimator, out cutsceneCamera);
                originalParent = cameraRig.transform.parent;
                originalPosition = cameraRig.transform.localPosition;
                originalRotation = cameraRig.transform.localRotation;
                cameraRig.transform.SetParent(cameraSource);
                cameraRig.transform.localPosition = Vector3.zero;
            }
        }
    }

    IEnumerator MoveModel()
{
    var anim = model.GetComponentInChildren<Animator>();

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
                // Constrain rotation to vertical (yaw) only and rotate via shortest path
                float targetYaw = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
                float currentYaw = model.eulerAngles.y;
                float newYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, rotationSpeed * Time.deltaTime);
                model.rotation = Quaternion.Euler(0f, newYaw, 0f);
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
        cameraRig.transform.parent = originalParent;
        cameraRig.transform.localPosition = originalPosition;
        cameraRig.transform.localRotation = originalRotation;
        cutsceneCamera.transform.localRotation = Quaternion.identity;

        CallNext();
    }
}

IEnumerator AlignRotation(Quaternion targetRotation)
{
    Quaternion startRot = model.rotation;
    float startYaw = startRot.eulerAngles.y;
    float targetYaw = targetRotation.eulerAngles.y;
    float duration = 0.4f;
    float t = 0f;

    while (t < duration)
    {
        float yaw = Mathf.LerpAngle(startYaw, targetYaw, t / duration);
        model.rotation = Quaternion.Euler(0f, yaw, 0f);
        t += Time.deltaTime;
        yield return null;
    }

    model.rotation = Quaternion.Euler(0f, targetYaw, 0f);
}


}
