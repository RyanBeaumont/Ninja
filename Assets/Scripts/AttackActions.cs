using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using Unity.VisualScripting;

public class Attack
{
    public string attackName = "";
    public float cooldown = 0f;
    public int cancelPriority = 0;
    public float range = 2f;
    public List<AttackAction> attackActions = new List<AttackAction>();

    public int GetDuration()
    {
        int total = 0;
        foreach(AttackAction action in attackActions)
        {
            total += action.duration;
        }
        return total;
    }
}

public class AttackActions : MonoBehaviour
{
    public static AttackActions Instance;
    List<Attack> attacks = new List<Attack>();
    void Start()
    {
        Instance = this;

        //Jab
        var n = "Jab";
        attacks.Add(new Attack()
        {
            attackName = n, cooldown = 0f, cancelPriority = 0, range = 0f,
            attackActions = new List<AttackAction>
            {
                new LungeAction { anim = n, duration = 4, distance = 0.5f, state = State.Windup },
                new HitboxAction { duration = 5, state = State.Active, hitbox = Resources.Load<GameObject>($"Hitboxes/{n}") },
                new WaitAction { duration = 9, state = State.FollowThrough }
            }
        });

        //Punch
        n = "Punch";   
        attacks.Add(new Attack()
        {
            attackName = n, cooldown = 0f, cancelPriority = 1, range = 2.5f,
            attackActions = new List<AttackAction>
            {
                new LungeAction { anim = n, duration = 10, distance = 1f, state = State.Windup },
                new HitboxAction { duration = 2, state = State.Active, hitbox = Resources.Load<GameObject>($"Hitboxes/{n}") },
                new WaitAction { duration = 15, state = State.FollowThrough }
            }
        });

        //Punch
        n = "PunchCombo";   
        attacks.Add(new Attack()
        {
            attackName = n, cooldown = 0f, cancelPriority = 2, range = 3f,
            attackActions = new List<AttackAction>
            {
                new LungeAction { anim = n, duration = 10, distance = 1.5f, state = State.Windup },
                new HitboxAction { duration = 1, state = State.Active, hitbox = Resources.Load<GameObject>($"Hitboxes/{n}") },
                new LungeAction { duration = 9, distance = 1f,state = State.Active },
                new HitboxAction { duration = 1, state = State.Active, hitbox = Resources.Load<GameObject>($"Hitboxes/{n}") },
                new LungeAction { duration = 14,distance = 1f, state = State.Active },
                new HitboxAction { duration = 1, state = State.Active, hitbox = Resources.Load<GameObject>($"Hitboxes/PunchComboFinal") },
                new WaitAction { duration = 18, state = State.FollowThrough }
            }
        });

        //Kick
        n = "Kick";
        attacks.Add(new Attack()
        {
            attackName = n, cooldown = 0f, cancelPriority = 3, range = 4f,
            attackActions = new List<AttackAction>
            {
                new LungeAction { anim = n, duration = 14, distance = 2f, state = State.Windup },
                new HitboxAction { duration = 6, state = State.Active, hitbox = Resources.Load<GameObject>($"Hitboxes/{n}") },
                new WaitAction { duration = 24, state = State.FollowThrough }
            }
        });
        
        //Uppercut
        n = "Uppercut";
        attacks.Add(new Attack()
        {
            attackName = n, cooldown = 0f, cancelPriority = 3, range = 2f,
            attackActions = new List<AttackAction>
            {
                new LungeAction { anim = n, duration = 14, distance = 1.5f, state = State.Windup },
                new WaitForHitHitboxAction { duration = 60, state = State.Active, hitbox = Resources.Load<GameObject>($"Hitboxes/{n}"),distance = 8f},
                new WaitAction {anim = "Uppercut", duration = 20, state = State.FollowThrough }
            }
        });

        //Uppercut
        n = "Sweep";
        attacks.Add(new Attack()
        {
            attackName = n, cooldown = 0f, cancelPriority = 3, range = 2f,
            attackActions = new List<AttackAction>
            {
                new LungeAction { anim = n, duration = 14, distance = 1f, state = State.Windup },
                new HitboxAction { duration = 2, state = State.Active, hitbox = Resources.Load<GameObject>($"Hitboxes/{n}") },
                new WaitAction { duration = 33, state = State.FollowThrough }
            }
        });
        
    }

    public Attack GetAttack(string name)
    {
        foreach(Attack attack in attacks)
        {
            if(attack.attackName == name)
            {
                return attack;
            }
        }
        return null;
    }
}



public abstract class AttackAction
{
    public int duration = 1;
    public State state = State.Idle;
    public string anim = "";
    public abstract void Execute(GameObject user);
}

public class WaitAction : AttackAction
{
     public override void Execute(GameObject user)
    {
        
    }
}

public class WaitForGroundHitboxAction : AttackAction
{
    public GameObject hitbox = null;
    public float distance = 0f;
    public float ySpeed = 0f;
    public override void Execute(GameObject user)
    {
        var character = user.GetComponent<Character>();
        if(character == null) return;
        character.waitForGround = true;

        if(hitbox != null){
        var h = Object.Instantiate(hitbox,user.transform);
        var hit = h.transform.GetComponentInChildren<Hitbox>();
        hit.owner = user;
        hit.timer = duration;
        h.transform.localPosition = Vector3.zero;
        h.transform.localRotation = Quaternion.identity;
    }

        if(distance != 0f){
            //Debug.Log($"Lunge for frames: {duration}");
            float seconds = duration * (1f/50f);
            float rate = distance/seconds;
            //take into account player's facing direction
            Vector3 vel = user.transform.forward * rate;
            vel.y = ySpeed;
            //Debug.Log($"Lunging with rate {rate} m/s");
            character.Lunge(vel,seconds);
        }
    }
}


public class HitboxAction : AttackAction
{
    public GameObject hitbox = null;

    public override void Execute(GameObject user)
    {
        if(hitbox == null) return;
        var h = Object.Instantiate(hitbox,user.transform);
        var hit = h.transform.GetComponentInChildren<Hitbox>();
        hit.owner = user;
        hit.timer = duration;
        h.transform.localPosition = Vector3.zero;
        h.transform.localRotation = Quaternion.identity;
    }
}

public class WaitForHitHitboxAction : AttackAction
{
    public GameObject hitbox = null;
    public float distance = 0f;
    public float ySpeed = 0f;
    public override void Execute(GameObject user)
    {
        var character = user.GetComponent<Character>();
        if(hitbox == null) return; if(character == null) return;

        character.waitForHit = true;
        var h = Object.Instantiate(hitbox,user.transform);
        var hit = h.transform.GetComponentInChildren<Hitbox>();
        hit.owner = user;
        hit.timer = duration;
        h.transform.localPosition = Vector3.zero;
        h.transform.localRotation = Quaternion.identity;

        if(distance != 0f){
            //Debug.Log($"Lunge for frames: {duration}");
            float seconds = duration * (1f/50f);
            float rate = distance/seconds;
            //take into account player's facing direction
            Vector3 vel = user.transform.forward * rate;
            vel.y = ySpeed;
            //Debug.Log($"Lunging with rate {rate} m/s");
            character.Lunge(vel,seconds);
        }
    }
}


public class LungeAction : AttackAction
{
    public float distance = 0f;
    public float ySpeed = 0f;

    public override void Execute(GameObject user)
    {
        var character = user.GetComponent<Character>();
        
        if(character == null) return;
        //Debug.Log($"Lunge for frames: {duration}");
        float seconds = duration * (1f/50f);
        float rate = distance/seconds;
        //take into account player's facing direction
        Vector3 vel = user.transform.forward * rate;
        vel.y = ySpeed;
        //Debug.Log($"Lunging with rate {rate} m/s");
        character.Lunge(vel,seconds);
    }

}

public class KnockbackAction : AttackAction
{
    public Vector3 direction;
    public float distance = 0f;
    public float ySpeed = 0f;

    public override void Execute(GameObject user)
    {
        var character = user.GetComponent<Character>();
        
        if(character == null) return;
        //Debug.Log($"Lunge for frames: {duration}");
        float seconds = duration * (1f/50f);
        float rate = distance/seconds;
        //take into account player's facing direction

        Vector3 vel = direction.normalized * rate;
        vel.y = ySpeed;
        //Debug.Log($"Lunging with rate {rate} m/s");
        character.Lunge(vel,seconds);
    }

}