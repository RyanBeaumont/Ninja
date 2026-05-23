using UnityEngine;

public class CopyAnimator : MonoBehaviour
{
    [Header("References")]
    public Animator sourceAnimator;
    public Animator targetAnimator;

    [Header("Settings")]
    public int layer = 0;

    private int lastStateHash;

    void LateUpdate()
    {
        if (sourceAnimator == null || targetAnimator == null)
            return;

        AnimatorStateInfo sourceState = sourceAnimator.GetCurrentAnimatorStateInfo(layer);

        int currentHash = sourceState.fullPathHash;

        // Only replay animation if state changed
        if (currentHash != lastStateHash)
        {
            targetAnimator.Play(
                currentHash,
                layer,
                sourceState.normalizedTime % 1f
            );

            lastStateHash = currentHash;
        }

        // Keep playback speed synchronized
        targetAnimator.speed = sourceAnimator.speed;

        // Continuously sync animation time
        AnimatorStateInfo targetState = targetAnimator.GetCurrentAnimatorStateInfo(layer);

        float sourceTime = sourceState.normalizedTime;
        float targetTime = targetState.normalizedTime;

        // Small correction threshold to prevent jitter/restarting every frame
        if (Mathf.Abs(sourceTime - targetTime) > 0.05f)
        {
            targetAnimator.Play(
                currentHash,
                layer,
                sourceTime
            );
        }
    }
}