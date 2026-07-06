using UnityEngine;
using UnityEngine.UI;

public class PromptIcon : MonoBehaviour
{
    public string iconName;
    Image image;
        void Start()
        {
            GameManager.Instance.inputIconsChanged += RefreshPromptIcon;
            image = GetComponent<Image>();
            RefreshPromptIcon();
        }

    void RefreshPromptIcon()
    {
        if (image != null)
        {
            image.sprite = Resources.Load<Sprite>($"{GameManager.Instance.inputIconsPath}/{iconName}");
        }
    }
}
