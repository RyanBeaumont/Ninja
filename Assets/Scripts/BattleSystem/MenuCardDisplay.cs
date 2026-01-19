
using UnityEngine.EventSystems;
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

