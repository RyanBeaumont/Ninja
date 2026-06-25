using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
public class ShowStats : MonoBehaviour
{
    public Transform statsPanel;
    public Image combatantIcon;
    public TMP_Text combatantName;
    public TMP_Text combatantStats;
    public float threshold = 100f;
    Transform currentTarget;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
         statsPanel.gameObject.SetActive(false);
    }

    // Update is called once per frame
   void Update()
    {
        List<GameObject> candidates = new List<GameObject>();
        var targeter = FindFirstObjectByType<Targeter>();
        if (targeter != null)
        {
            var target = targeter.ActiveTarget;
            if (target != null)
            {
                UpdateUI(target);
                return;
            }
        }

        if (BattleManager.Instance.actionQueue.Count > 0) { statsPanel.gameObject.SetActive(false); return; }

        candidates = BattleManager.Instance.combatants
            .Select(c => c.gameObject)
            .ToList();

    // Step 2: Convert to Combatants and filter
        var filtered = candidates
            .Select(go => go.GetComponent<Combatant>())
            .Where(c => c != null)
            .Where(c => c is EnemyCombatant)
            .Where(c => c.alive) // if targetDead=true → alive must be false
            .Select(c => c.gameObject);
            // Passively move the targeter to the closest matching object to the mouse cursor

            if(filtered != null && filtered.Count() > 0){
                var cam = Camera.main;
                if(cam != null){
                    var mousePos = Input.mousePosition;
                    float bestDistSqr = float.MaxValue;
                    Transform best = null;
                    var thresholdSqr = threshold * threshold;
                    foreach(var go in filtered){
                        if(go == null) continue;
                        var screenPos = cam.WorldToScreenPoint(go.transform.position);
                        // skip objects behind the camera
                        if(screenPos.z <= 0) continue;
                        var dx = screenPos.x - mousePos.x;
                        var dy = screenPos.y - mousePos.y;
                        var distSqr = dx*dx + dy*dy;
                        if(distSqr < bestDistSqr){
                            bestDistSqr = distSqr;
                            best = go.transform;
                        }
                    }
                    
                    if(best != null && bestDistSqr <= thresholdSqr)
                    {
                        if (currentTarget != best)
                        {
                            currentTarget = best;
                            UpdateUI(currentTarget);
                        }
                    }
                    else
                    {
                        currentTarget = null;
                        var currentPlayer = BattleManager.Instance.activePlayer;
                        if(currentPlayer != null)
                        {
                            UpdateUI(currentPlayer.transform);
                        }
                    }
                }
            }
            else
            {
                currentTarget = null;
                var currentPlayer = BattleManager.Instance.activePlayer;
                if(currentPlayer != null)
                {
                    UpdateUI(currentPlayer.transform);
                }
            }
    }

    void UpdateUI(Transform target)
    {
        statsPanel.gameObject.SetActive(false);
        
        if(target == null){return;}
        var combatant = target.GetComponentInChildren<Combatant>();
        if(combatant == null) return;
        statsPanel.gameObject.SetActive(true);
        var icon = Resources.Load<Sprite>($"Sprites/{combatant.combatantName}");
        if(combatant is EnemyCombatant enemyCombatant) icon = enemyCombatant.portrait;
        if(icon != null) combatantIcon.sprite = icon;
        combatantName.text = combatant.combatantName;
        combatantStats.text = $"HP: {Mathf.Round(combatant.hp)}/{combatant.maxHp}    MP: {Mathf.Round(combatant.mp)}/{combatant.maxMp}";
        //delete all children of statsPanel except "CharacterStats"
        foreach(Transform child in statsPanel){
            if(child.name != "CharacterStats"){
                Destroy(child.gameObject);
            }
        }
        foreach(DamageType dt in combatant.discoveredResistances){
            var prefab = Instantiate(Resources.Load<GameObject>("Discovery"),statsPanel);
            prefab.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/Cards/{dt.ToString()}Damage");
            var nameText = prefab.transform.Find("EffectName").GetComponent<TMP_Text>();
            nameText.text = "Resistant to "+dt.ToString();
        }
        foreach(DamageType dt in combatant.discoveredWeaknesses){
            var prefab = Instantiate(Resources.Load<GameObject>("Discovery"),statsPanel);
            prefab.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/Cards/{dt.ToString()}Damage");
            var nameText = prefab.transform.Find("EffectName").GetComponent<TMP_Text>();
            nameText.text = "Weak to "+dt.ToString();
        }
        // Add status effects
        foreach(StatusEffect se in combatant.statusEffects)
        {
            var prefab = Instantiate(Resources.Load<GameObject>("StatusEffectUI"),statsPanel);
            prefab.transform.Find("Image").GetComponent<Image>().sprite = se.sprite;
            var nameText = prefab.transform.Find("EffectName").GetComponent<TMP_Text>();
            nameText.text = se.name;
            var descriptionText = prefab.transform.Find("EffectDescription").GetComponent<TMP_Text>();
            var description = "";
            if(se.duration != -1) description += $"({se.duration} turns) ";
            description += se.description;
            //Show the description but replace "[amount]" with se.amount and [duration] with se.duration and evaluate any simple expressions like [amount*2] or [duration+1]    
                description = System.Text.RegularExpressions.Regex.Replace(description, @"\[(\w+)\s*([+\-*\/])?\s*(\d+(?:\.\d+)?)?\]", match => {
                    var statName = match.Groups[1].Value;
                    var op = match.Groups[2].Value;
                    var numberStr = match.Groups[3].Value;
                    float statValue = 0;
                    if(statName == "amount"){
                        statValue = (float)se.amount;
                    }else if(statName == "duration"){
                        statValue = (float)se.duration;
                    }
                    if(op != "" && numberStr != ""){
                        float number = float.Parse(numberStr);
                        switch(op){
                            case "+":
                                statValue += number;
                                break;
                            case "-":
                                statValue -= number;
                                break;
                            case "*":
                                statValue *= number;
                                break;
                            case "/":
                                statValue /= number;
                                break;
                        }
                    }
                    return statValue % 1 == 0 ? statValue.ToString("F0") : statValue.ToString();
                });
            descriptionText.text = description;
        }
    }
}

