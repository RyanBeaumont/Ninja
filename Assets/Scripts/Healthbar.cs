using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class Healthbar : MonoBehaviour {
    Character character;
    Combatant combatant;
    Slider whiteHealth;
    Image hpBar;
    Slider mp;
    TMP_Text levelText;
    TMP_Text hpText;
    TMP_Text mpText;
    Slider health;
    public float referenceDistance = 1f;
    public float baseScale = 1f;

    void Start()
    {
        combatant = GetComponentInParent<Combatant>();
        hpBar = transform.Find("Fill/Healthbar").GetComponent<Image>();
        whiteHealth = transform.Find("Background").GetComponent<Slider>();
        health = transform.Find("Fill").GetComponent<Slider>();
        mp = transform.Find("MP").GetComponent<Slider>();
        levelText = transform.Find("Level/LevelText").GetComponent<TMP_Text>();
        hpText = transform.Find("HPValue").GetComponent<TMP_Text>();
        mpText = transform.Find("MPValue").GetComponent<TMP_Text>();
        if(combatant != null)
        {
            if(combatant is PlayerCombatant)
            {
                //Lime green
                hpBar.color = new Color(0.5f, 1f, 0f);
            }
            whiteHealth.maxValue = combatant.maxHp;
            health.maxValue = combatant.maxHp;
            mp.maxValue = combatant.maxMp;
            levelText.text = combatant.level.ToString();
        }
    }

    void Update()
    {
        if(combatant != null) 
        {
            health.value = Mathf.RoundToInt(combatant.hp);
            hpText.text = Mathf.RoundToInt(combatant.hp).ToString();
            health.value = Mathf.RoundToInt(combatant.hp);
            mp.value = Mathf.RoundToInt(combatant.mp);
            mpText.text = Mathf.RoundToInt(combatant.mp).ToString();
        }
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

