using UnityEngine;
using Unity.Cinemachine;

public class CameraControl : MonoBehaviour
{
    public FloatValue xSensitivity;
    public FloatValue ySensitivity;

    CinemachineInputAxisController controller;

    void Start()
    {
        controller = GetComponent<CinemachineInputAxisController>();

        xSensitivity.OnValueChanged += OnHSensitivityChanged;
        ySensitivity.OnValueChanged += OnVSensitivityChanged;

        // Apply initial values
        OnHSensitivityChanged(xSensitivity.value);
        OnVSensitivityChanged(ySensitivity.value);
    }

    void OnDestroy()
    {
        if (xSensitivity != null)
            xSensitivity.OnValueChanged -= OnHSensitivityChanged;
        if (ySensitivity != null)
            ySensitivity.OnValueChanged -= OnVSensitivityChanged;
    }

    void OnHSensitivityChanged(float newH)
    {
        SetAxisMultiplier("Look X (Pan)", newH);
    }

    void OnVSensitivityChanged(float newV)
    {
        SetAxisMultiplier("Look Y (Tilt)", -newV);
    }

    void SetAxisMultiplier(string axisName, float value)
    {
        if (controller == null) return;

        foreach (var axis in controller.Controllers)
        {
            if (axis.Name == axisName)
            {
                axis.Input.Gain = value; // <-- Unity 6 field
                return;
            }
        }

        Debug.LogWarning($"Axis '{axisName}' not found.");
    }
}