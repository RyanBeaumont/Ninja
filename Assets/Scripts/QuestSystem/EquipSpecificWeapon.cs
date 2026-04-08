using UnityEngine;

public class EquipSpecificWeapon : ChainedInteractable
{
    public Transform character;
    public GameObject weapon;

    public override void Interact()
    {
        if(character == null) character = GameObject.FindGameObjectWithTag("Player").transform;
        character.GetComponentInChildren<DefaultPose>().combatWeapon = weapon;
        CallNext();
    }
}
