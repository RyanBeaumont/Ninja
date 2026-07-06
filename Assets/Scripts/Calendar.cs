using UnityEngine;
using UnityEngine.InputSystem;

public class Calendar : MonoBehaviour
{
    public RectTransform panel;
    public bool dayAdvanced = false;

    void Start()
    {
        if (panel == null)
        {
            Debug.LogWarning("Calendar: panel is not assigned.");
            return;
        }
        if (YourParty.instance == null)
        {
            Debug.LogWarning("Calendar: YourParty.instance is null.");
            return;
        }

        var day = YourParty.instance.day;
        foreach (Transform child in panel.transform)
        {
            if (child == null) continue;

            // Use the name of the child to determine which day it is
            var name = child.name ?? string.Empty;
            if (!name.StartsWith("Day ")) continue;

            var numPart = name.Substring(4).Trim();
            if (!int.TryParse(numPart, out int childDay))
            {
                var m = System.Text.RegularExpressions.Regex.Match(numPart, "\\d+");
                if (m.Success)
                    childDay = int.Parse(m.Value);
                else
                    continue;
            }

            var textTransform = child.Find("Number");
            var slashTransform = child.Find("Slash");

            if (childDay == day)
            {
                if (textTransform != null && textTransform.TryGetComponent<TMPro.TMP_Text>(out var tmp))
                    tmp.text = $"<color=yellow>{day}</color>";
            }
            else
            {
                if (childDay < day && slashTransform != null)
                    slashTransform.gameObject.SetActive(true);

                if (textTransform != null && textTransform.TryGetComponent<TMPro.TMP_Text>(out var tmp2))
                    tmp2.text = $"{childDay}";
            }
        }
    }

    void Update()
    {
        if (Input.GetButton("Interact") && !dayAdvanced)
        {
            if (YourParty.instance == null || panel == null) return;
            dayAdvanced = true;
            var day = YourParty.instance.day;
            foreach (Transform child in panel.transform)
            {
                if (child == null) continue;
                if (child.name != $"Day ({day})") continue;

                var slash = child.Find("Slash");
                if (slash != null)
                    slash.gameObject.SetActive(true);

                var shaker = GetComponentInChildren<UIShake>();
                if (shaker != null)
                    shaker.Shake();

                AudioManager.Instance?.PlaySoundEffect("s_punch");
                break;
            }
        }
    }
}
