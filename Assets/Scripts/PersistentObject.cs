using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    [HideInInspector] public string encounterID;
    [HideInInspector] public bool active = true;
    protected virtual void Awake()
    {
        encounterID = $"{gameObject.scene.name}_{transform.position}";
    }

    public virtual void DisableObject()
    {
        GameManager.Instance.AddEncounter($"{gameObject.scene.name}_{transform.position}");
        active = false;
    }
}
