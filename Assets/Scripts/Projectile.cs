using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Projectile : MonoBehaviour
{
    public string tagToHit = "Enemy";
    int indexToHit = 0;
    Combatant target;
    public float speed = 10f;
    bool initialized = false;
    string[] directions = {"Left","Right","Jump","Duck"};

    public void Initialize(string targetTag)
    {
        tagToHit = targetTag;
        initialized = true;
        var list = GameObject.FindGameObjectsWithTag(tagToHit)
        .Select(go => go.GetComponent<Combatant>())
        .Where(c => c != null && c.alive)
        .ToList();
        if(list.Count > 0)
        {
            indexToHit = Random.Range(0,list.Count);
            target = list[indexToHit];
            BattleManager.Instance.currentTargets = new List<Combatant>(){target};
        }
        Invoke("SafetyDestroy",2f);
    }

    void Update()
    {
        if(!initialized) return;
        if(target == null) SafetyDestroy();
        transform.LookAt(target.transform);
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if(!initialized) return;
        if (other.gameObject.CompareTag(tagToHit))
        {
            Combatant c = other.GetComponent<Combatant>();
            if(c != null)
            {
                BattleManager.Instance.currentTargets = new List<Combatant>(){c};
                if(tagToHit == "Enemy")
                    BattleManager.Instance.PlayerHit();
                else
                    BattleManager.Instance.EnemyHit(directions[indexToHit]);
            }
            Destroy(gameObject);
        }
    }

    void SafetyDestroy()
    {
        if(tagToHit == "Enemy")
            BattleManager.Instance.PlayerHit();
        else
            BattleManager.Instance.EnemyHit(directions[indexToHit]);
        Destroy(gameObject);
    }
}
