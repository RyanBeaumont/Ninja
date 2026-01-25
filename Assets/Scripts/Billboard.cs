using UnityEngine;

public class Billboard : MonoBehaviour
{
    public virtual void Update()
    {
        transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}
