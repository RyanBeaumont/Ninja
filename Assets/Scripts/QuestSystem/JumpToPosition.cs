using UnityEngine;

public class JumpToPosition : ChainedInteractable
{
    public Transform model;
    public Transform destination;
    public override void Interact()
    {
        if(model == null) model = GameObject.FindGameObjectWithTag("Player").transform;
        model.transform.position = destination.transform.position;
        model.transform.rotation = destination.transform.rotation;
        CallNext();
    }
}