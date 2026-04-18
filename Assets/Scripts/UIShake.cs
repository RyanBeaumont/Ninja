using System.Collections;
using UnityEngine;

public class UIShake : MonoBehaviour{
    float shake = 0f;
    Vector3 originalPosition;
    public void Shake(float s = 10){shake = s;}

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void Update()
    {
        // If shake is active, apply a random offset to the UI element's position
        if (shake > 0f)
        {
            float offsetX = Random.Range(-shake, shake);
            float offsetY = Random.Range(-shake, shake);
            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            // Gradually reduce the shake intensity over time
            shake -= Time.deltaTime * 15f; // Adjust the decay rate as needed
        }
        else
        {
            shake = 0f; // Ensure shake doesn't go negative
        }
    }
    
}
