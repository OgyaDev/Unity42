using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;


public class CameraController : NetworkBehaviour
{
    public Transform torso; // The upper body torso transform
    public float rotationSpeed = 5f;
    
    private float yaw = 0f;
    private float pitch = 0f;

    void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // Update yaw and pitch
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        // Rotate the torso
        torso.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
