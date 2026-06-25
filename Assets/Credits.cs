using UnityEngine;
using TMPro;
public class Credits : MonoBehaviour
{
    public TMP_Text creditsText;
    public float scrollSpeed = 20f;

    // Update is called once per frame
    void Update()
    {
        //Scroll the credits upward
        creditsText.transform.Translate(Vector3.up * scrollSpeed * Time.deltaTime);
    }
}
