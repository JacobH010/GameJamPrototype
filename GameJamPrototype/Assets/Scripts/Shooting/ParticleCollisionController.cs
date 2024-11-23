using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionController : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnParticleCollision(GameObject other)
    {
        AIController aiController = other.GetComponent<AIController>();

        if (aiController != null)
        {
            aiController.KillEnemy();
            
        }
    }
}
