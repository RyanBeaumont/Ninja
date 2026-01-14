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
    public Vector3 targetLocalPos;
    public Quaternion targetLocalRot;
    public float smoothFactor = 0.125f;
    public float horOffset = 2f;
    public float vertOffset = 0.5f;
    public Card card;

    public virtual void SetData(Card card)
    {
        this.card = card;
        cardImage.sprite = Resources.Load<Sprite>($"Sprites/Cards/{card.artwork}");
        nameText.text = card.cardName;
        descriptionText.text = card.description;
        costText.text = card.cost.ToString();
        if(card.cardClass == CardClass.Warrior) borderImage.sprite = Resources.Load<Sprite>("Sprites/Cards/WarriorBorder");
        if(card.cardClass == CardClass.Grappler) borderImage.sprite = Resources.Load<Sprite>("Sprites/Cards/SupportBorder");
        if(card.cardClass == CardClass.Ninja) borderImage.sprite = Resources.Load<Sprite>("Sprites/Cards/NinjaBorder");
        if(card.cardClass == CardClass.Psychic) borderImage.sprite = Resources.Load<Sprite>("Sprites/Cards/PsychicBorder");
        if(card.tpCost > 0) borderImage.sprite = Resources.Load<Sprite>("Sprites/Cards/UltimateBorder");
        if(card.effects.Count > 0 && card.effects[0] is DamageAction d)
        {
            if(d.damageType == DamageType.Slashing) damageTypeImage.sprite = Resources.Load<Sprite>("Sprites/Cards/SlashingDamage");
            if(d.damageType == DamageType.Bludgeoning) damageTypeImage.sprite = Resources.Load<Sprite>("Sprites/Cards/BludgeoningDamage");
            if(d.damageType == DamageType.Psychic) damageTypeImage.sprite = Resources.Load<Sprite>("Sprites/Cards/PsychicDamage");
        }
        
        selectedBorder.enabled = false;
    }


    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        selectedBorder.enabled = true;
        targetLocalPos += new Vector3(0f, 20f, 0f);
    }
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        selectedBorder.enabled = false;
        targetLocalPos -= new Vector3(0f, 20f, 0f);
    }
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        var activePlayer = BattleManager.Instance.activePlayer;
        if (activePlayer != null)
        {
            if(activePlayer.PlayCard(card))
                Destroy(gameObject);
        }
    }
    void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, smoothFactor);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetLocalRot, smoothFactor);
    }
}

