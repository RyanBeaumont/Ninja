using UnityEngine;
using System.Collections;

public class PartyLight : MonoBehaviour
{
    Light light1;
    void Start()
    {
        light1 = GetComponent<Light>();
        StartCoroutine(PartyLights());
    }

    IEnumerator PartyLights()
    {
        yield return new WaitForSeconds(Random.Range(0f,1f));
        while (true)
        {
            yield return new WaitForSeconds(1f);
            light1.color = new Color(Random.Range(0,255),Random.Range(0,255),Random.Range(0,255),255);
        }
    }
}
