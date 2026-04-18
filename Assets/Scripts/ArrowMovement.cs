using UnityEngine;
using UnityEngine.UI;

public class ArrowMovement : MonoBehaviour
{
   public Arrow thisArrow;
   public float beats = 4;
   public QuickTimeEvent qt;
   public Vector3 startScale;
   public float spawnTime;

    public virtual void Start()
    {
        
        spawnTime = Time.time;
        startScale = transform.localScale;
        transform.localScale *= 2f;
        qt = FindFirstObjectByType<QuickTimeEvent>();
        thisArrow = (Arrow)Random.Range(0,4);
        if(thisArrow == Arrow.Up) transform.Rotate(new Vector3(0f,0f,90f));
        if(thisArrow == Arrow.Left) transform.Rotate(new Vector3(0f,0f,180f));
        if(thisArrow == Arrow.Down) transform.Rotate(new Vector3(0f,0f,270f));
    }

    public virtual void Update()
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
        float curvedT = Mathf.Clamp01(t * t); // Clamp to prevent overshoot during acceleration

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

    }

    public float ProcessHit(Arrow arrow)
    {
        if(arrow == thisArrow)
        {
            var timeOff = Mathf.Abs((Time.time-spawnTime) - (beats * qt.beatLength));
            return timeOff;
            
        }
        else
        {
            return 100;
        }
    }

}
