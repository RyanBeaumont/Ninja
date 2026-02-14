using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class CardDisplay : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image cardImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text costText;
    public Image borderImage;
    public Image damageTypeImage;
    public Image selectedBorder;
    public Image discardImage;
    public Image tpImage;
    public TMP_Text discardCost;
    public Vector3 targetLocalPos;
    public Transform damageTypeOverlay;
    public Quaternion targetLocalRot;
    public float smoothFactor = 0.125f;
    public float horOffset = 2f;
    public float vertOffset = 0.5f;
    int originalSiblingIndex;
    public Card card;

    public virtual void SetData(Card card)
    {
        this.card = card;
        cardImage.sprite = Resources.Load<Sprite>($"Sprites/Cards/{card.artwork}");
        nameText.text = card.cardName;
        descriptionText.text = card.description;
        costText.text = card.cost.ToString();
        if(card.tempCost != 0)
        {
            costText.text = card.tempCost.ToString();
        }
        if(card.discardCost > 0)
        {
            discardCost.text = card.discardCost.ToString();
        }
        else
        {
            discardImage.gameObject.SetActive(false);
        }
        if(card.tpCost > 0)
        {
            costText.text = $"{card.tpCost}";
            costText.color = Color.cyan;
        }
        else
        {
            tpImage.gameObject.SetActive(false);
        }
        
        if(card.cardClass == CardClass.Warrior) borderImage.sprite = Resources.Load<Sprite>("Sprites/Cards/WarriorBorder");
        if(card.cardClass == CardClass.Grappler) borderImage.sprite = Resources.Load<Sprite>("Sprites/Cards/SupportBorder");
        if(card.cardClass == CardClass.Ninja) borderImage.sprite = Resources.Load<Sprite>("Sprites/Cards/NinjaBorder");
        if(card.cardClass == CardClass.Psychic) borderImage.sprite = Resources.Load<Sprite>("Sprites/Cards/PsychicBorder");
        if(card.tpCost > 0) borderImage.sprite = Resources.Load<Sprite>("Sprites/Cards/UltimateBorder");
        if(card.effects.Count > 0 && card.effects[0] is DamageAction d)
        {
            if(d.damageType == DamageType.Slashing){
                var s = Resources.Load<Sprite>("Sprites/Cards/SlashingDamage");
                damageTypeImage.sprite = s;
                if(damageTypeOverlay != null){
                damageTypeOverlay.GetComponent<Image>().sprite = s;
                damageTypeOverlay.GetComponentInChildren<TMP_Text>().text = "Slashing Damage";
                }
            }
            if(d.damageType == DamageType.Bludgeoning)
            {
                var s = Resources.Load<Sprite>("Sprites/Cards/BludgeoningDamage");
                damageTypeImage.sprite = s;
                if(damageTypeOverlay != null){
                damageTypeOverlay.GetComponent<Image>().sprite = s;
                damageTypeOverlay.GetComponentInChildren<TMP_Text>().text = "Bludgeoning Damage";
                }
            } 
            if(d.damageType == DamageType.Psychic)
            {
                var s = Resources.Load<Sprite>("Sprites/Cards/PsychicDamage");
                damageTypeImage.sprite = s;
                if(damageTypeOverlay != null){
                damageTypeOverlay.GetComponent<Image>().sprite = s;
                damageTypeOverlay.GetComponentInChildren<TMP_Text>().text = "Psychic Damage";
                }
            } 
        }
        
        if(damageTypeOverlay != null) damageTypeOverlay.gameObject.SetActive(false);
        selectedBorder.enabled = false;
    }


    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        selectedBorder.enabled = true;
        targetLocalPos += new Vector3(0f, 20f, 0f);
        originalSiblingIndex = transform.GetSiblingIndex();
        if(FindFirstObjectByType<BattleManager>() != null){
            transform.SetAsLastSibling();
            if(damageTypeOverlay != null) damageTypeOverlay.gameObject.SetActive(true);
        }
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        selectedBorder.enabled = false;
        targetLocalPos -= new Vector3(0f, 20f, 0f);
        transform.SetSiblingIndex(originalSiblingIndex);
        if(damageTypeOverlay != null) damageTypeOverlay.gameObject.SetActive(false);
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left){
            var activePlayer = BattleManager.Instance.activePlayer;
            if (activePlayer != null)
            {
                if(activePlayer.PlayCard(card))
                    Destroy(gameObject);
            }
        }
        else
        {
            var activePlayer = BattleManager.Instance.activePlayer;
            if (activePlayer != null)
            {
                activePlayer.DiscardCard(card);
                Destroy(gameObject);
            }
        }
    }

    void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, smoothFactor);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetLocalRot, smoothFactor);
    }
}

