using Unity.Mathematics;
using UnityEngine;

public class QuestIcon : Billboard
{
    bool canAlert = false;
    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        var player = FindFirstObjectByType<Character>().gameObject;
        if(player != null)
        {
            float distToPlayer = (transform.position - player.transform.position).magnitude;
            if(distToPlayer < 5f)
            {
                if (canAlert)
                {
                    canAlert = false;
                    AudioManager.Instance.PlaySoundEffect("Alert");
                }
            }
            else
            {
                canAlert = true;
            }

        }
    }
}
