using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombinationLockDigit : MonoBehaviour
{
    public TextMeshProUGUI numberText;
    public Button plusButton;
    public Button minusButton;

    public int Value { get; private set; }

    private int min = 0;
    private int max = 9;

    private void Awake()
    {
        plusButton.onClick.AddListener(Increment);
        minusButton.onClick.AddListener(Decrement);
        UpdateDisplay();
    }

    public void Increment()
    {
        Value++;
        if (Value > max) Value = min;
        UpdateDisplay();
    }

    public void Decrement()
    {
        Value--;
        if (Value < min) Value = max;
        UpdateDisplay();
    }

    public void SetValue(int value)
    {
        Value = Mathf.Clamp(value, min, max);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        AudioManager.Instance.PlaySoundEffect("MenuEquip");
        numberText.text = Value.ToString();
    }
}
