using UnityEngine;

public class AudioEncounter : ChainedInteractable
{
    public AudioClip encounterMusic;
    public float fadeTime = 1.5f;
    public string sfx;
    public override void Interact()
    {
       if (encounterMusic != null) AudioManager.Instance.PlayMusic(encounterMusic,fadeTime);
       if (sfx != null) AudioManager.Instance.PlaySoundEffect(sfx);
       CallNext();
    }
}
