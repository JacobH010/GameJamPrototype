using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        RenderSettings.ambientIntensity = .05f;
        RenderSettings.reflectionIntensity = 0f;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
