using System;
using TMPro;
using UnityEngine;

using UnityEngine.UI;
using System.Text.RegularExpressions;

[Serializable] public class SearchResult
{
    public string name;
    //rich text box
    [TextArea(3, 10)] public string body;
}

public class ComputerTerminal : MonoBehaviour
{
    public Button searchButton;
    public GameObject searchResultPrefab;
    public Transform resultsContainer;
    public TMP_InputField inputField;
    public GameObject activeArticle;
    public TMP_Text articleTitle;
    public TMP_Text articleBody;
    public SearchResult[] searchResults;

    void Start()
    {
        searchButton.onClick.AddListener(Search);
        Clear();
    }

    void Search()
    {
        var searchString = inputField.text;
        Clear();
        bool foundMatch = false;
        if(searchString.Length < 4)
        {
            AudioManager.Instance.PlaySoundEffect("Negative");
            articleTitle.text = "Search term too short";
            articleBody.GetComponent<TMP_Text>().text = "The infinitely wise ninja council requires at least 4 characters to process your request";
            activeArticle.SetActive(true);
            return;
        }
        //search both content and body for matches and return all matches
        foreach (var result in searchResults)
        {
            if (ContainsWord(result.name, searchString) || ContainsWord(result.body, searchString))
            {
                foundMatch = true;
                var resultObj = Instantiate(searchResultPrefab, resultsContainer);

                resultObj.transform.Find("Title").GetComponentInChildren<TMP_Text>().text = HighlightTerm(result.name, searchString);
                resultObj.transform.Find("Quote").GetComponentInChildren<TMP_Text>().text = HighlightTerm(GetPreview(result.body, searchString), searchString);

                var resultButton = resultObj.GetComponent<Button>();

                var capturedResult = result;

                resultButton.onClick.AddListener(() => {
                    Clear();
                    AudioManager.Instance.PlaySoundEffect("MenuEquip");
                    articleTitle.text = capturedResult.name;
                    articleBody.text = capturedResult.body;
                    activeArticle.SetActive(true);
                });
            }
        }

        if(!foundMatch)
        {
            AudioManager.Instance.PlaySoundEffect("Negative");
            articleTitle.text = "No Results";
            articleBody.text = "Even the infinitely wise ninja council are completely confounded by your request";
            activeArticle.SetActive(true);
        }
    }

    string HighlightTerm(string body, string search)
    {
        if (string.IsNullOrEmpty(search)) return body;

        return System.Text.RegularExpressions.Regex.Replace(
            body,
            search,
            $"<b>{search}</b>",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
    }

    bool ContainsWord(string text, string word)
    {
        //not strict match
        //return text.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0;
        return Regex.IsMatch(
            text,
            $@"\b{Regex.Escape(word)}\b",
            RegexOptions.IgnoreCase
        );
        
    }

    string GetPreview(string body, string search, int contextLength = 20)
    {

            return body.Substring(0, Mathf.Min(50, body.Length)) + "...";
    }

    void Clear()
    {
        foreach(Transform child in resultsContainer){Destroy(child.gameObject);}
        activeArticle.SetActive(false);
    }
    

}
