using UnityEngine;

public class StartMusic : MonoBehaviour
{
    public AudioClip musicClip;
    public AudioClip encounterClip;
    public string locationName;

    void Start()
    {
        AudioManager.Instance.StartCoroutine(AudioManager.Instance.FadeToNewTheme(musicClip, encounterClip));
    }
}
