using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.InputSystem;

public class HandManager : MonoBehaviour
{
    public Transform handTransform; //root of hand position
    public float fanSpread = 5f;
    public float cardSpacing = 5f;
    public float maxHandWidth = 800f; // total width the hand can occupy
    public float maxFanSpread = 5f; // max rotation spread
    public float verticalCardSpacing = 0.18f;
    public List<GameObject> cardsInHand = new List<GameObject>();

    public void InitializeHand(List<Card> cards)
    {
        foreach (GameObject child in cardsInHand) Destroy(child);
        cardsInHand.Clear();
        foreach (Card card in cards)
        {
            AddCardToHand(card);
        }
    }

    public void AddCardToHand(Card card)
    {
        GameObject newCard = Instantiate(Resources.Load<GameObject>("CardPrefab"), handTransform.position, Quaternion.identity, handTransform);
        cardsInHand.Add(newCard);
        newCard.GetComponent<CardDisplay>().SetData(card);
        UpdateHandVisuals();
    }

    void Update()
    {
        //UpdateHandVisuals();
    }

    GameObject ResolveHpBar(PlayerCombatant playerCombatant)
    {
        if (playerCombatant.hpBar != null) return playerCombatant.hpBar;
        var hpBarTransform = playerCombatant.transform.Find("Healthbar") ?? playerCombatant.transform.Find("Health");
        if (hpBarTransform == null) return null;
        playerCombatant.hpBar = hpBarTransform.gameObject;
        return playerCombatant.hpBar;
    }

    public void SetHandActive(bool isActive)
    {
        handTransform.gameObject.SetActive(isActive);
        foreach (var playerCombatant in BattleManager.Instance.combatants.OfType<PlayerCombatant>())
        {
            var hpBar = ResolveHpBar(playerCombatant);
            if (hpBar == null) continue;
            hpBar.SetActive(!isActive);
        }

        //Select the first card in hand for controller navigation if hand is active
        if (isActive && cardsInHand.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(cardsInHand[0]);
        }
        if(cardsInHand.Count == 0)
        {
            //Find "Skip Turn" button and select it
            var skipTurnButton = GameObject.Find("Pass");
            EventSystem.current.SetSelectedGameObject(skipTurnButton);
        }
    }

    public void UpdateHandVisuals()
{
    int cardCount = cardsInHand.Count;
    if (cardCount == 0) return;

    float handWidth = Mathf.Min(maxHandWidth, cardCount * cardSpacing);
    float step = (cardCount > 1) ? handWidth / (cardCount - 1) : 0;

    for (int i = 0; i < cardCount; i++)
    {
        RectTransform rt = cardsInHand[i].GetComponent<RectTransform>();
        CardDisplay cd = cardsInHand[i].GetComponent<CardDisplay>();

        float t = (cardCount > 1) ? i / (float)(cardCount - 1) : 0.5f;
        float centered = t * 2f - 1f;

        float rotationAngle = centered * maxFanSpread;
        float x = -handWidth / 2f + step * i;
        float y = verticalCardSpacing * (1 - centered * centered);

        cd.targetLocalPos = new Vector3(x, y, 0f);
        cd.targetLocalRot = Quaternion.Euler(0f, 0f, rotationAngle);

        // keep draw order left→right
        cardsInHand[i].transform.SetSiblingIndex(i);
    }
}
}
