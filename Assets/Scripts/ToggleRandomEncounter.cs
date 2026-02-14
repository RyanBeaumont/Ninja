using UnityEngine;

public class ToggleRandomEncounter : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.GetComponentInParent<Character>() != null)
        {
            FindFirstObjectByType<RandomEncounter>().globalRandomEncounters = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.GetComponentInParent<Character>() != null)
        {
            FindFirstObjectByType<RandomEncounter>().globalRandomEncounters = false;
        }
    }
}
