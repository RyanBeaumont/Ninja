using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using Mono.Cecil.Cil;
using System;

public class MenuCardDisplay : CardDisplay
{

    Menu menu;
    public Action onPointerDown;

    public override void SetData(Card card)
    {
        base.SetData(card);
        menu = GetComponentInParent<Menu>();
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        onPointerDown?.Invoke();
    }
    void Update()
    {
        
    }
}

