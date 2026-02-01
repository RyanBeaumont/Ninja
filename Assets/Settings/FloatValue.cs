using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Values/FloatValue")]
public class FloatValue : ScriptableObject
{
    [SerializeField] private float defaultValue;
    [SerializeField] private float _value;

    public event Action<float> OnValueChanged;

    string PrefKey => name; // uses the asset name as key

    public float value
    {
        get => _value;
        set
        {
            if (Mathf.Approximately(_value, value)) return;

            _value = value;
            PlayerPrefs.SetFloat(PrefKey, _value);
            PlayerPrefs.Save();

            OnValueChanged?.Invoke(_value);
        }
    }

    private void OnEnable()
    {
        // Load from PlayerPrefs when asset is enabled
        _value = PlayerPrefs.GetFloat(PrefKey, defaultValue);
    }
}
