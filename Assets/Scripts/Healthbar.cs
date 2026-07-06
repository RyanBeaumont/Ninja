using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class Healthbar : MonoBehaviour {
    Character character;
    public Combatant combatant;
    public Slider whiteHealth;
    public Image hpBar;
    public RectTransform ultReady;
    Slider mp;
    TMP_Text levelText;
    public TMP_Text hpText;
    TMP_Text mpText;
    public Slider health;
    public TMP_Text nameText;
    public float baseHP = 300f;
    public float baseScale = 1f;

    private Combatant lastCombatant;

    void Start()
    {
        ultReady.gameObject.SetActive(false);
        if(combatant == null) combatant = GetComponentInParent<Combatant>();
        if(hpText == null) hpText = transform.Find("HPValue").GetComponent<TMP_Text>();
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
            if (combatant.mp >= combatant.maxMp)
            {
                //ultReady.gameObject.SetActive(true);
            }
            else
            {
                ultReady.gameObject.SetActive(false);
            }
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

