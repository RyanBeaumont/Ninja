using UnityEngine;

[RequireComponent(typeof(Light))]
public class CandleLight : MonoBehaviour
{
    private Light candleLight;
    private float baseIntensity;

    [Header("Flicker Settings")]
    [Tooltip("Maximum change from base intensity")]
    public float intensityVariance = 0.3f;
    [Tooltip("How quickly the light flickers")]
    public float flickerSpeed = 10f;
    [Tooltip("How smoothly the flicker blends (0 = choppy, 1 = smooth)")]
    [Range(0f, 1f)]
    public float smoothness = 0.8f;

    private float noiseOffset;

    void Start()
    {
        candleLight = GetComponent<Light>();
        baseIntensity = candleLight.intensity;
        noiseOffset = Random.Range(0f, 100f); // makes each light flicker differently
    }

    void Update()
    {
        // Use Perlin noise for natural flicker
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseOffset);
        float targetIntensity = baseIntensity + (noise - 0.5f) * 2f * intensityVariance;

        // Optional: smooth out changes
        candleLight.intensity = Mathf.Lerp(candleLight.intensity, targetIntensity, 1f - Mathf.Pow(1f - smoothness, Time.deltaTime * 60f));
    }
}
