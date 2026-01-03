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
    public Image selectedBorder;
    public Vector3 targetLocalPos;
    public Quaternion targetLocalRot;
    public float smoothFactor = 0.125f;
    public float horOffset = 2f;
    public float vertOffset = 0.5f;
    public Card card;

    public void SetData(Card card)
    {
        this.card = card;
        cardImage.sprite = card.artwork;
        nameText.text = card.cardName;
        descriptionText.text = card.description;
        costText.text = card.cost.ToString();
        selectedBorder.enabled = false;
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        selectedBorder.enabled = true;
        targetLocalPos += new Vector3(0f, 20f, 0f);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        selectedBorder.enabled = false;
        targetLocalPos -= new Vector3(0f, 20f, 0f);
    }
    public void OnPointerDown(PointerEventData eventData)
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

