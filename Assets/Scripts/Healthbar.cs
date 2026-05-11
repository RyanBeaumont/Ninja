using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class Healthbar : MonoBehaviour {
    Character character;
    public Combatant combatant;
    public Slider whiteHealth;
    Image hpBar;
    Slider mp;
    TMP_Text levelText;
    public TMP_Text hpText;
    TMP_Text mpText;
    public Slider health;
    public TMP_Text nameText;
    public float baseHP = 300f;
    public float baseScale = 1f;

    void Start()
    {
        //mp = transform.Find("MP").GetComponent<Slider>();
        //levelText = transform.Find("Level/LevelText").GetComponent<TMP_Text>();
        if(hpText != null) hpText = transform.Find("HPValue").GetComponent<TMP_Text>();
        //mpText = transform.Find("MPValue").GetComponent<TMP_Text>();
        if(combatant != null)
        {
            if(combatant is PlayerCombatant)
            {
                //Lime green
                hpBar.color = new Color(0.5f, 1f, 0f);
            }
            whiteHealth.maxValue = combatant.maxHp;
            health.maxValue = combatant.maxHp;
            
            //mp.maxValue = combatant.maxMp;
            //levelText.text = combatant.level.ToString();

            //Scale width to mimic max health
            //transform.localScale = new Vector3(baseScale * (combatant.maxHp / baseHP), baseScale, baseScale);
        }
    }

    void Update()
    {
        if(combatant != null) 
        {
            health.value = Mathf.RoundToInt(combatant.hp);
            if(hpText != null) hpText.text = Mathf.RoundToInt(combatant.hp).ToString();
            if(nameText != null) nameText.text = $"{combatant.name} (HP: {Mathf.Round(combatant.hp)}/{Mathf.Round(combatant.maxHp)})";
            //mp.value = Mathf.RoundToInt(combatant.mp);
            //mpText.text = Mathf.RoundToInt(combatant.mp).ToString();
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

