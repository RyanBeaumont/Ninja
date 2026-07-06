using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class CardDisplay : Selectable,
    IPointerDownHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,
    IDeselectHandler,
    ISubmitHandler,
    ICancelHandler
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
    public bool displayMode;
    public float displaySelectedScale = 1.05f;
    public Color displaySelectedColor = new Color(1f, 0.95f, 0.6f, 1f);
    public Color displayNormalColor = Color.white;
    public Vector3 targetLocalPos;
    public Transform damageTypeOverlay;
    public Quaternion targetLocalRot;
    public float smoothFactor = 0.125f;
    public float horOffset = 2f;
    public float vertOffset = 0.5f;

    int originalSiblingIndex;
    bool isHighlighted;

    public Card card;
    public Action onSubmitAction;
    public Action onCancelAction;

    public virtual void SetData(Card card)
    {
        this.card = card;

        cardImage.sprite = Resources.Load<Sprite>($"Sprites/Cards/{card.artwork}");
        nameText.text = card.cardName;
        descriptionText.text = card.description;
        costText.text = card.cost.ToString();

        if (card.tempCost != 0)
        {
            costText.text = card.tempCost.ToString();
        }

        if (card.discardCost > 0)
        {
            discardCost.text = card.discardCost.ToString();
        }
        else
        {
            discardImage.gameObject.SetActive(false);
        }

        if (card.tpCost > 0)
        {
            costText.text = $"{card.tpCost}";
            costText.color = Color.cyan;
        }
        else
        {
            tpImage.gameObject.SetActive(false);
        }

        if (card.cardClass == CardClass.Warrior)
            borderImage.sprite = Resources.Load<Sprite>("Sprites/Cards/WarriorBorder");

        if (card.cardClass == CardClass.Grappler)
            borderImage.sprite = Resources.Load<Sprite>("Sprites/Cards/SupportBorder");

        if (card.cardClass == CardClass.Ninja)
            borderImage.sprite = Resources.Load<Sprite>("Sprites/Cards/NinjaBorder");

        if (card.cardClass == CardClass.Psychic)
            borderImage.sprite = Resources.Load<Sprite>("Sprites/Cards/PsychicBorder");

        if (card.tpCost > 0)
            borderImage.sprite = Resources.Load<Sprite>("Sprites/Cards/UltimateBorder");

        if (card.effects.Count > 0 && card.effects[0] is DamageAction d)
        {
            if (d.damageType == DamageType.Slashing)
            {
                
                var s = Resources.Load<Sprite>("Sprites/Cards/SlashingDamage");
                damageTypeImage.sprite = s;
                if (damageTypeOverlay != null)
                {
                    
                    damageTypeOverlay.GetComponent<Image>().sprite = s;
                    damageTypeOverlay.GetComponentInChildren<TMP_Text>().text = "Slashing Damage";
                }
            }

            if (d.damageType == DamageType.Bludgeoning)
            {
                
                var s = Resources.Load<Sprite>("Sprites/Cards/BludgeoningDamage");
                damageTypeImage.sprite = s;
                if (damageTypeOverlay != null)
                {
                    
                    damageTypeOverlay.GetComponent<Image>().sprite = s;
                    damageTypeOverlay.GetComponentInChildren<TMP_Text>().text = "Bludgeoning Damage";
                }
            }

            if (d.damageType == DamageType.Psychic)
            {
               
                var s = Resources.Load<Sprite>("Sprites/Cards/PsychicDamage");
                damageTypeImage.sprite = s;
                if (damageTypeOverlay != null)
                {
                    
                    damageTypeOverlay.GetComponent<Image>().sprite = s;
                    damageTypeOverlay.GetComponentInChildren<TMP_Text>().text = "Psychic Damage";
                }
            }

            if(d.damageType == DamageType.None)
            {
                var s = Resources.Load<Sprite>("Sprites/Cards/NoDamage");
                    damageTypeImage.sprite = s;
                if (damageTypeOverlay != null)
                {
                    
                    damageTypeOverlay.GetComponent<Image>().sprite = s;
                    damageTypeOverlay.GetComponentInChildren<TMP_Text>().text = "Raw Damage";
                }
            }
        }else{
            //Disable damageTypeImage for non-damage cards
            var s = Resources.Load<Sprite>("Sprites/Cards/NoDamage");
            damageTypeImage.sprite = s;
            if (damageTypeOverlay != null)
                {
            
            damageTypeOverlay.GetComponent<Image>().sprite = s;
            damageTypeOverlay.GetComponentInChildren<TMP_Text>().text = "";
                }
        }

        if (damageTypeOverlay != null)
            damageTypeOverlay.gameObject.SetActive(false);

        selectedBorder.enabled = false;
    }

    private void Highlight()
    {
        if (isHighlighted)
            return;

        isHighlighted = true;

        if (displayMode)
        {
            ApplyDisplaySelectionVisual(true);
            return;
        }

        if (BattleManager.Instance == null)
            return;

        selectedBorder.enabled = true;


        /*
        if (BattleManager.Instance.activePlayer.mp > card.cost &&
            BattleManager.Instance.activePlayer.tp > card.tpCost)
        {
            selectedBorder.color = Color.white;
        }
        else
        {
            selectedBorder.color = Color.red;
        }
        */

        targetLocalPos += new Vector3(0f, 20f, 0f);

        originalSiblingIndex = transform.GetSiblingIndex();

        transform.SetAsLastSibling();

        if (damageTypeOverlay != null)
            damageTypeOverlay.gameObject.SetActive(true);
    }

    private void Unhighlight()
    {
        if (!isHighlighted)
            return;

        isHighlighted = false;

        if (displayMode)
        {
            ApplyDisplaySelectionVisual(false);
            return;
        }

        selectedBorder.enabled = false;

        targetLocalPos -= new Vector3(0f, 20f, 0f);

        transform.SetSiblingIndex(originalSiblingIndex);

        if (damageTypeOverlay != null)
            damageTypeOverlay.gameObject.SetActive(false);
    }

    private void ApplyDisplaySelectionVisual(bool selected)
    {
        if (cardImage != null)
        {
            cardImage.color = selected ? displaySelectedColor : displayNormalColor;
        }

        if (borderImage != null)
        {
            borderImage.color = selected ? displaySelectedColor : displayNormalColor;
        }

        if (selectedBorder != null)
        {
            selectedBorder.enabled = selected;
        }

        if (Application.isPlaying)
        {
            var targetScale = new Vector3(0.8f,0.8f,1f);
            transform.localScale = selected ? targetScale * displaySelectedScale : targetScale;
        }
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (displayMode)
        {
            Highlight();
            EventSystem.current?.SetSelectedGameObject(gameObject);
            return;
        }

        AudioManager.Instance.PlaySoundEffect("MenuHover");

        if (FindFirstObjectByType<BattleManager>() == null)
            return;

        Highlight();

        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (displayMode)
        {
            Unhighlight();
            return;
        }

        if (EventSystem.current.currentSelectedGameObject != gameObject)
        {
            Unhighlight();
        }
    }

    public override void OnSelect(BaseEventData eventData)
    {
        Highlight();
    }

    public override void OnDeselect(BaseEventData eventData)
    {
        Unhighlight();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (displayMode)
        {
            if (eventData.button == PointerEventData.InputButton.Left && onSubmitAction != null)
            {
                onSubmitAction.Invoke();
            }
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            PlayCard();
        }
        else
        {
            DiscardCard();
        }
    }

    public virtual void OnSubmit(BaseEventData eventData)
    {
        if (displayMode)
        {
            onSubmitAction?.Invoke();
            return;
        }

        PlayCard();
    }

    public void OnCancel(BaseEventData eventData)
    {
        if (displayMode)
        {
            onCancelAction?.Invoke();
            return;
        }

        DiscardCard();
    }

    private void DiscardCard()
    {
        var activePlayer = BattleManager.Instance.activePlayer;

        if (activePlayer != null)
        {
            activePlayer.DiscardCard(card);
            GameManager.Instance.SelectDefault();
            Destroy(gameObject);
        }
    }

    private void PlayCard()
    {
        var activePlayer = BattleManager.Instance.activePlayer;

        if (activePlayer != null)
        {
            if (activePlayer.PlayCard(card))
            {
                Destroy(gameObject);
            }
        }
    }

    private void CheckControllerRightClick()
    {
        if (!isHighlighted)
            return;

        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != gameObject)
            return;

        if (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            var activePlayer = BattleManager.Instance.activePlayer;

            if (activePlayer != null)
            {
                activePlayer.DiscardCard(card);
                GameManager.Instance.SelectDefault();
                Destroy(gameObject);
            }
        }
    }

    void Update()
    {
        if (displayMode)
        {
            return;
        }

        CheckControllerRightClick();

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetLocalPos,
            smoothFactor);

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetLocalRot,
            smoothFactor);
    }
}