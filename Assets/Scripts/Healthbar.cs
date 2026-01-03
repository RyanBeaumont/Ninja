using UnityEngine;

using UnityEngine.UI;

public class Healthbar : MonoBehaviour {
    Character character;
    Combatant combatant;
    Slider whiteHealth;
    Slider health;

    void Start()
    {
        character = GetComponentInParent<Character>();
        combatant = GetComponentInParent<Combatant>();
        whiteHealth = transform.Find("Background").GetComponent<Slider>();
        health = transform.Find("Fill").GetComponent<Slider>();
        if(character != null)
        {
            whiteHealth.maxValue = character.maxHp;
            health.maxValue = character.maxHp;
        }
        if(combatant != null)
        {
            whiteHealth.maxValue = combatant.maxHp;
            health.maxValue = combatant.maxHp;
        }
    }

    void Update()
    {
        if(character != null) health.value = character.hp;
        if(combatant != null) health.value = combatant.hp;
        if(whiteHealth.value > health.value)
        {
            whiteHealth.value -= Time.deltaTime * 30f;
        }
        else
        {
            whiteHealth.value = health.value;
        }
    }
}

