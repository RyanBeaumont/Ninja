using UnityEngine;

public class TheaterSpotlight : MonoBehaviour
{
    public Transform focusPoint;   // where the light aims (center of stage)
    public float radius = 5f;       // how wide it sweeps
    public float speed = 0.5f;      // movement speed
    public float height = 8f;       // height above focus point
    public float wobble = 0.5f;     // randomness

    private float timeOffset;

    void Start()
    {
        timeOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float t = Time.time * speed + timeOffset;

        float x = Mathf.Sin(t) * radius;
        float z = Mathf.Cos(t * 0.7f) * radius;

        // optional wobble
        x += Mathf.PerlinNoise(t, 0f) * wobble - wobble / 2f;
        z += Mathf.PerlinNoise(0f, t) * wobble - wobble / 2f;

        Vector3 targetPos = focusPoint.position + new Vector3(x, height, z);
        transform.position = targetPos;

        transform.LookAt(focusPoint.position);
    }
}