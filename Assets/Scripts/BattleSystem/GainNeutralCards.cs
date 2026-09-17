using UnityEngine;
using System.Collections.Generic;

public class GainNeutralCards : ChainedInteractable
{
    public override void Interact()
    {
        var allCards = CardDatabase.Instance.GetCardsByClass(CardClass.None,99);
        List<string> cardNames = new List<string>();
        foreach(var card in allCards)
        {
            YourParty.instance.AddClasslessCard(card.cardName, 2);
        }
        
        CallNext();
    }
}
