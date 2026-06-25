
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

    public override void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySoundEffect("MenuHover");
        // Intentionally not calling base to skip card reordering/movement
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        // Intentionally not calling base to skip card reset behavior
    }
}

