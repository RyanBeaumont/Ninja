using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.InputSystem;

public class HandManager : MonoBehaviour
{
    public Transform handTransform; //root of hand position
    public float fanSpread = 5f;
    public float cardSpacing = 5f;
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

    public void SetHandActive(bool isActive)
    {
        handTransform.gameObject.SetActive(isActive);
    }

    public void UpdateHandVisuals()
    {
        int cardCount = cardsInHand.Count;
        for (int i = 0; i < cardCount; i++)
        {
            float rotationAngle = (fanSpread * (i - (cardCount - 1) / 2f));
            cardsInHand[i].GetComponent<CardDisplay>().targetLocalRot = Quaternion.Euler(0f, 0f, rotationAngle);
            float horizontalOffset = 0f; float normalizedPosition = 0f;
            if (cardCount > 1)
            {
                horizontalOffset = i * cardSpacing;
                normalizedPosition = (2f * i / (cardCount - 1) - 1f);
            }
            float verticalOffset = verticalCardSpacing * (1 - normalizedPosition * normalizedPosition);
            cardsInHand[i].GetComponent<CardDisplay>().targetLocalPos = new Vector3(horizontalOffset, verticalOffset, 0f);
        }
    }
}
