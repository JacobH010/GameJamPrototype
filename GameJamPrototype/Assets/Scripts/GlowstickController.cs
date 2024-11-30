using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowstickController : MonoBehaviour
{
    public GameObject glowstickPrefab;
    public GameObject glowstickWorldObject;
    // Start is called before the first frame update
    void Start()
    {
 
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Instantiate<GameObject>(glowstickPrefab, GlowstickSpawnLocation());
        }
    }
    private Transform GlowstickSpawnLocation()
    {
        
        return glowstickWorldObject.transform;

    }
}
