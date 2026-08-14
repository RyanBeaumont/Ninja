using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class Projectile : MonoBehaviour
{
    public string tagToHit = "Enemy";
    int indexToHit = 0;
    Combatant target;
    public float speed = 10f;
    bool initialized = false;
    string[] directions;
    //List<GameObject> prompts;

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
        if(list.Count == 1)  directions = new string[]{"Jump"};
        else if(list.Count == 2)  directions = new string[]{"Left","Right"};
        else if(list.Count == 3)  directions = new string[]{"Left","Jump","Right"};
        else if(list.Count == 4)  directions = new string[]{"Left","Jump","Duck","Right"};
        if(list.Count > 0 && list[0] is PlayerCombatant){
            for(int i=0; i<list.Count; i++)
            {
                var prompt = Instantiate(Resources.Load<GameObject>("DodgePrompt"), list[i].transform);
                prompt.transform.localPosition = new Vector3(0, 0, 0);
                string text = "";
                if(directions[i] == "Jump") text = "W";
                if(directions[i] == "Duck") text = "S";
                if(directions[i] == "Left") text = "A";
                if(directions[i] == "Right") text = "D";
                prompt.GetComponentInChildren<TMP_Text>().text = text;
                Destroy(prompt, 2f);
            }
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
