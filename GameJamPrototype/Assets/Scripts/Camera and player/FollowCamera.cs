using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform cameraTransform;

    private void LateUpdate()
    {
        // Set the position and rotation to match the camera's view
        transform.position = cameraTransform.position;
        transform.rotation = cameraTransform.rotation;
    }
}
