using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class Healthbar : MonoBehaviour {
    public Combatant combatant;
    public Slider whiteHealth;
    public Image hpBar;
    public Transform mpGauge;
    public TMP_Text hpText;
    public Slider health;
    public TMP_Text nameText;
    private Combatant lastCombatant;
    private float flashTimer = 0;
    float lastMP = 0;
    bool flash = false;

    void Start()
    {
        if(combatant == null) combatant = GetComponentInParent<Combatant>();
        UpdateHealthbarForCombatant();
    }

    void UpdateHealthbarForCombatant()
    {
        if (combatant == null) return;
        if (combatant != lastCombatant)
        {
            lastCombatant = combatant;
            if (combatant is PlayerCombatant)
            {
                Debug.Log("Player combatant found, setting hp bar color to lime green");
                hpBar.color = new Color(0.5f, 1f, 0f);
            }
        }

        whiteHealth.maxValue = combatant.maxHp;
        health.maxValue = combatant.maxHp;
        health.value = Mathf.RoundToInt(combatant.hp);
        whiteHealth.value = health.value;
        UpdateMPBar(combatant);

    }

    public void UpdateMPBar(Combatant combatant)
    {
        if(combatant is EnemyCombatant){
            lastMP = combatant.mp;
            foreach(Transform child in mpGauge) Destroy(child.gameObject);
            for(int i=0; i<combatant.maxMp; i++)
            {
                var mpInstance = Instantiate(Resources.Load<GameObject>("MPIcon"),mpGauge);
                if(combatant.mp > i){mpInstance.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Sparkle");}
            }
        }
    }

    void Update()
    {
        if (combatant != null)
        {
            if (combatant != lastCombatant || health.maxValue != combatant.maxHp || whiteHealth.maxValue != combatant.maxHp)
            {
                UpdateHealthbarForCombatant();
            }

            health.value = Mathf.RoundToInt(combatant.hp);
            if (hpText != null) hpText.text = Mathf.RoundToInt(combatant.hp).ToString();
            if (nameText != null) nameText.text = $"{combatant.combatantName} (HP: {Mathf.Round(combatant.hp)}/{Mathf.Round(combatant.maxHp)})";
            if (combatant.mp >= combatant.maxMp && combatant.maxMp > 0)
            {
                if(flashTimer <= 0f)
                {
                    flash = !flash;
                    foreach(Transform child in mpGauge)
                    {
                        if(flash)
                            child.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/WhiteMana");
                        else
                            child.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/EmptyMana");
                    }
                    flashTimer = 0.5f;
                }
                flashTimer -= Time.deltaTime;
            }
            else
            {
                flash = false;
            }
            if(lastMP != combatant.mp){UpdateMPBar(combatant);}
        }
        else
        {
            Debug.LogWarning("No combatant assigned to healthbar");
        }

        if (whiteHealth.value > health.value)
        {
            whiteHealth.value -= Time.deltaTime * 30f;
        }
        else
        {
            whiteHealth.value = health.value;
        }
    }

}

