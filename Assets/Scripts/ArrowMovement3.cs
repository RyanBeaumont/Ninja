using UnityEngine;
using UnityEngine.UI;
public class ArrowMovement3 : ArrowMovement
{
    float startRotation;
    public override void Start()
    {
        base.Start();
        AudioManager.Instance.PlaySoundEffect("Whimsy");
        transform.Rotate(new Vector3(0f,0f,-180f));
        startRotation = transform.eulerAngles.z;

    }
    public override void Update()
    {
        if(transform.localScale.x > startScale.x){
            var newScale = transform.localScale.x - Time.deltaTime * 2f;
            transform.localScale = new Vector3(newScale,newScale,newScale);
        }
        var beatLength = qt.beatLength * beats;
        var offset = qt.trackLength;

        float elapsed = Time.time - spawnTime;
        float t = elapsed / beatLength;

        // Ease-in (slow → fast) for the first beat
        float curvedT = Mathf.Clamp01(t); // Clamp to prevent overshoot during acceleration

        float x;
        if (t <= 1f)
        {
            // First beat: ease-in to hit zone
            x = Mathf.Lerp(offset, 0f, curvedT);
        }
        else
        {
            GetComponent<Image>().color = Color.red;
            // After first beat: continue moving at constant speed
            float extraTime = t - 1f;
            float speed = offset / beatLength; // Speed needed to reach hit zone in one beat
            x = 0f - (speed * extraTime);
        }

        transform.localPosition = new Vector3(
            x,
            transform.localPosition.y,
            transform.localPosition.z
        );

        //rotate 180 degrees over the course of travel
        float rotation = Mathf.Lerp(0f,180f,t);
        transform.rotation = Quaternion.Euler(0f,0f,startRotation + rotation);

    }

}
