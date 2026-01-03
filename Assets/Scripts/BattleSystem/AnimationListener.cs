using UnityEngine;
public class AnimationListener : MonoBehaviour
{
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        
    }

    void Hit(string direction)
    {
        if(GetComponentInParent<EnemyCombatant>() != null)
        {
            GetComponentInParent<EnemyCombatant>().OnHit(direction);
        }
        else if(GetComponentInParent<PlayerCombatant>() != null)
        {
            GetComponentInParent<PlayerCombatant>().OnHit();
        }
    }

    void SlowMo()
    {
        if(GetComponentInParent<EnemyCombatant>() != null)
        {
            var effect = Instantiate(Resources.Load<GameObject>("Particles/HitLight"), transform);
            //Time.timeScale = 0.125f;
        }
    }
}
