using UnityEngine;
using System.Collections;
public class AudioManager : MonoBehaviour
{
     [SerializeField] AudioSource sourceA;
    [SerializeField] AudioSource sourceB;

    AudioSource active;
    AudioSource inactive;
    float musicVolume = 0.4f;
     [SerializeField] AudioSource soundEffectsSource;

     
    [SerializeField]AudioClip mainTheme;
    [SerializeField]AudioClip encounterTheme;

    public static AudioManager Instance { get; private set; }

    void Start()
    {
        active = sourceA;
        inactive = sourceB;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayMainTheme(){PlayMusic(mainTheme,1f);}
    public void PlayEncounterTheme(){PlayMusic(encounterTheme);}

    public IEnumerator FadeToNewTheme(AudioClip newMainTheme, AudioClip newEncounterTheme)
    {
        inactive.clip = newMainTheme;
        inactive.volume = 0f;
        inactive.Play();

        yield return StartCoroutine(Crossfade(1f));
        mainTheme = newMainTheme;
        encounterTheme = newEncounterTheme;
    }

    public void PlaySoundEffect(string effect, float pitch = 1f)
    {
        var fx = Resources.Load<AudioClip>($"Sound/SFX/{effect}");
        if(fx != null)
            soundEffectsSource.PlayOneShot(fx, pitch);
    }

    public void PlayMusic(AudioClip newClip, float fadeTime = 1.5f)
    {
        Debug.Log("PlayMusic called");
        if (active.clip == newClip)
            return;

        inactive.clip = newClip;
        inactive.volume = 0f;
        inactive.Play();

        StopAllCoroutines();
        StartCoroutine(Crossfade(fadeTime));
    }

    IEnumerator Crossfade(float duration)
    {
        float t = 0f;

        AudioSource from = active;
        AudioSource to = inactive;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = t / duration;

            from.volume = Mathf.Lerp(musicVolume, 0f, k);
            to.volume   = Mathf.Lerp(0f, musicVolume, k);

            yield return null;
        }

        from.volume = 0f;
        from.Stop();
        to.volume = 1f;

        //swap AFTER fade completes
        active = to;
        inactive = from;
    }


}
