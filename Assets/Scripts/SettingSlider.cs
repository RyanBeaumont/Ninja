using UnityEngine;
using UnityEngine.UI;

public class SettingSlider : MonoBehaviour
{
    public Slider slider;
    public FloatValue setting;
    void Start()
    {
        slider.value = setting.value;
    }

    public void OnSliderChanged()
    {
        setting.value = slider.value;
    }
}
