using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimbCollision : MonoBehaviour
{
    public PlayerCONTROLLER playerController;

    private void Start()
    {
        playerController = GameObject.FindObjectOfType<PlayerCONTROLLER>().GetComponent<PlayerCONTROLLER>();
    }

    private void OnCollisionExit(Collision other)
    {
        playerController.isGrounded = true;
    }
}
