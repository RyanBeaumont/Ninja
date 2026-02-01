using UnityEngine;
using System.Collections;
public class AudioManager : MonoBehaviour
{
     [SerializeField] AudioSource sourceA;
    [SerializeField] AudioSource sourceB;

    AudioSource active;
    AudioSource inactive;
     [SerializeField] AudioSource soundEffectsSource;

     
    [SerializeField]AudioClip mainTheme;
    [SerializeField]AudioClip encounterTheme;
    [SerializeField] FloatValue musicVolume;
    [SerializeField] FloatValue sfxVolume;

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
        musicVolume.OnValueChanged += OnVolumeChange;
        active.volume = musicVolume.value;
    }

    void OnVolumeChange(float newVolume)
    {
        print("VOLUME CHANGED");
        active.volume = newVolume;
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
        soundEffectsSource.volume = sfxVolume.value;
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

            from.volume = Mathf.Lerp(musicVolume.value, 0f, k);
            to.volume   = Mathf.Lerp(0f, musicVolume.value, k);

            yield return null;
        }

        from.volume = 0f;
        from.Stop();
        to.volume = musicVolume.value;

        //swap AFTER fade completes
        active = to;
        inactive = from;
    }


}
