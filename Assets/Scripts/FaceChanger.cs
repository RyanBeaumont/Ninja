using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FaceChanger : MonoBehaviour
{
    public List<Material> materials;
    public void ChangeFace(string face)
    {
        if(face == "") return;
        foreach(Material m in materials)
        {
            if(m.name == face)
            {
                //set this object's material to m
                GetComponent<MeshRenderer>().material = m;
                return;
            }
        }
        
    }
}
