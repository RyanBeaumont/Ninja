using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public float damage = 10f;
    public float knockbackDistance = 1f;
    public State hitState = State.Stunned;
    public bool finalHit = true;
    public AttackType attackType = AttackType.Mid;
    [HideInInspector] public GameObject owner;
    [HideInInspector] public int timer = -1;

    List<GameObject> alreadyDamaged = new List<GameObject>();

    void FixedUpdate()
    {
        timer --;
        if(timer <= 0 && timer != -1){Destroy(transform.parent.gameObject);}
    }

    void OnTriggerEnter(Collider other)
    {

        var hitChar = other.transform.GetComponent<Character>();
        var ownerChar = owner.GetComponent<Character>();

        if(hitChar != null && other.gameObject != owner && !alreadyDamaged.Contains(other.gameObject))
        {
            hitChar.TakeDamage(damage,owner.transform,knockbackDistance,hitState,attackType,finalHit);
            alreadyDamaged.Add(other.gameObject);

            if (ownerChar.waitForHit){ ownerChar.EndWaitForHit();   Destroy(transform.parent.gameObject);}
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            print("Hit wall");
            if(ownerChar != null && ownerChar.waitForHit){
                ownerChar.EndWaitForHit();
                Destroy(transform.parent.gameObject);
            }
        }
    }
}
