using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseIconBob : MonoBehaviour

{
    // Adjustable settings
    private float bobHeight = 0.5f; // Maximum height of the bobbing
    private float bobSpeed = 1f;    // Speed of the bobbing animation

    private Vector3 startPosition;

    void Start()
    {
        // Store the starting position of the object
        startPosition = transform.position;
    }

    void Update()
    {
        // Calculate the new Y position using a sine wave
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;

        // Apply the new position while keeping X and Z the same
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
