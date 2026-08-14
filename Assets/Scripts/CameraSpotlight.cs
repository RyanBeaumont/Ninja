using UnityEngine;
using System.Collections.Generic;

public class CameraSpotlight : MonoBehaviour
{
    public List<GameObject> targets = new List<GameObject>();
    private int activeTarget = -1;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.E))
        {
            return;
        }

        if (targets.Count == 0)
        {
            return;
        }

        GameObject previousTarget = null;
        if (activeTarget >= 0 && activeTarget < targets.Count)
        {
            previousTarget = targets[activeTarget];
        }

        activeTarget = (activeTarget + 1) % targets.Count;
        GameObject newTarget = targets[activeTarget];

        if (newTarget == null)
        {
            return;
        }

        for (int i = 0; i < targets.Count; i++)
        {
            GameObject target = targets[i];
            if (target == null)
            {
                continue;
            }

            if (i == activeTarget)
            {
                target.SetActive(true);
                transform.SetParent(target.transform, true);
                transform.position = target.transform.position;
                transform.rotation = target.transform.rotation;

                if (target.TryGetComponent(out DefaultPose defaultPose))
                {
                    defaultPose.PlayDefault();
                }
            }
            else if (previousTarget != null && previousTarget != newTarget)
            {
                target.SetActive(false);
            }
        }

        if (previousTarget != null && previousTarget != newTarget)
        {
            previousTarget.SetActive(false);
        }
    }
}
