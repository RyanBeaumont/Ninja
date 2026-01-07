using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using Mono.Cecil.Cil;
using System;

public class MenuCardDisplay : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image cardImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text costText;
    public Image borderImage;
    public Image selectedBorder;
    public Card card;
    Menu menu;
    public Action onPointerDown;

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
 
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        onPointerDown?.Invoke();
    }
    void Update()
    {
        
    }
}

