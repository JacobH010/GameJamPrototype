using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyShellDelete : MonoBehaviour
{
    public AudioSource shellFallSound;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Shell"))
        {
            shellFallSound.Play();
            Destroy(collision.gameObject);
        }
    }
}
