using UnityEngine;

public class StartMusic : MonoBehaviour
{
    public AudioClip musicClip;
    public AudioClip encounterClip;

    void Start()
    {
        AudioManager.Instance.StartCoroutine(AudioManager.Instance.FadeToNewTheme(musicClip, encounterClip));
    }
}
