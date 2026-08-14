using UnityEngine;

public class DeathTouch : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            AudioManager.Instance.PlaySoundEffect("explosion");
            YourParty.instance.LoadLastSave();
        }
    }
    
}
