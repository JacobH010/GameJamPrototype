using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingController : MonoBehaviour
{
    private static LightingController Instance;
    void Awake()
    {

        if (Instance == null)
        {
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); //prevents the manager from being destroyed when a level is loaded

                //Find the player in teh scene by tag
            }
            // Start is called before the first frame update
        }
        else
        {
            Destroy(gameObject);//destroys duplicate AIManagers
        }
    }
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
