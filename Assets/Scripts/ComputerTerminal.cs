using System;
using TMPro;
using UnityEngine;

using UnityEngine.UI;
using System.Text.RegularExpressions;

[Serializable] public class SearchResult
{
    public string name;
    public string body;
}

public class ComputerTerminal : MonoBehaviour
{
    public Button searchButton;
    public GameObject searchResultPrefab;
    public Transform resultsContainer;
    public TMP_InputField inputField;
    public GameObject activeArticle;
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
                    activeArticle.transform.Find("Title").GetComponent<TMP_Text>().text = capturedResult.name;
                    activeArticle.transform.Find("Body").GetComponent<TMP_Text>().text = capturedResult.body;
                    activeArticle.SetActive(true);
                });
            }
        }

        if(!foundMatch)
        {
            AudioManager.Instance.PlaySoundEffect("Negative");
            activeArticle.transform.Find("Title").GetComponent<TMP_Text>().text = "No Results";
            activeArticle.transform.Find("Body").GetComponent<TMP_Text>().text = "Even the infinitely wise ninja council are completely confounded by your request";
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
        return Regex.IsMatch(
            text,
            $@"\b{Regex.Escape(word)}\b",
            RegexOptions.IgnoreCase
        );
    }

    string GetPreview(string body, string search, int contextLength = 20)
    {
        if (string.IsNullOrEmpty(search)) return body.Substring(0, Mathf.Min(50, body.Length));

        int index = body.IndexOf(search, StringComparison.OrdinalIgnoreCase);

        if (index < 0)
        {
            // fallback if not found in body (but matched name)
            return body.Substring(0, Mathf.Min(50, body.Length)) + "...";
        }

        int start = Mathf.Max(0, index - contextLength);
        int end = Mathf.Min(body.Length, index + search.Length + contextLength);

        string snippet = body.Substring(start, end - start);

        if (start > 0) snippet = "..." + snippet;
        if (end < body.Length) snippet += "...";

        return snippet;
    }

    void Clear()
    {
        foreach(Transform child in resultsContainer){Destroy(child.gameObject);}
        activeArticle.SetActive(false);
    }
    

}
