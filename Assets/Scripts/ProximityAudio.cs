using UnityEngine;

public class ProximityAudio : MonoBehaviour
{
    public AudioClip audioClip;

    private void OnTriggerEnter(Collider other) {
        AudioManager.Instance.PlayMusic(audioClip,0.5f);
    }

    private void OnTriggerExit(Collider other) {
        AudioManager.Instance.PlayMainTheme();
    }
}
