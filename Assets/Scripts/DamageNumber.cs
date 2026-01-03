using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        //slowly rise up
        transform.position += new Vector3(0, 0.2f * Time.deltaTime, 0);
        //fade out
        var text = GetComponentInChildren<TMP_Text>();
        var color = text.color;
        color.a -= 0.5f * Time.deltaTime;
        text.color = color;
        if(color.a <= 0)
        {
            Destroy(gameObject);
        }
    }
}
