using UnityEngine;
using System.Collections;

public class RandomEncounterTrigger : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        var encounter = GameObject.FindFirstObjectByType<RandomEncounter>();
        if(encounter != null && other.CompareTag("Player"))
        {
            encounter.TriggerRandomEncounter(transform);
        }
    }
}
