using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCONTROLLER : MonoBehaviour
{
   public InputAction playerControl;
   public float speed;
   public float strafeSpeed;
   public float jumpForce;
   public Rigidbody hips;
   public bool isGrounded;
   public Transform orientation;
   private void OnEnable()
   {
      playerControl.Enable();
   }
   private void OnDisable()
   {
      playerControl.Disable();
   }

   private void Start()
   {
      hips = GetComponent<Rigidbody>();

   }
   [Header("--- ANIMATORS ---")]
   [SerializeField] private Animator _animatedAnimator;
   [SerializeField] private Animator _physicalAnimator;
   public Animator AnimatedAnimator {
      get { return _animatedAnimator; }
      private set { _animatedAnimator = value; }
   }

   private void Awake()
   {

   }


   private void FixedUpdate()
   {
      if (Input.GetKey(KeyCode.W))
      {
            
         if (Input.GetKey(KeyCode.LeftShift))
         {
            _animatedAnimator.SetBool("isWalking",true);
            _animatedAnimator.SetBool("isRunning",true);
            hips.AddForce(hips.transform.forward * speed * 1.5f);
         }
         else
         {
            hips.AddForce(hips.transform.forward * speed);
            _animatedAnimator.SetBool("isWalking",true);
            _animatedAnimator.SetBool("isRunning",false);
         }
       
      }
      else
      {
         _animatedAnimator.SetBool("isWalking",false);
         _animatedAnimator.SetBool("isRunning",false);
      }

      if (Input.GetKey(KeyCode.A))
      {
         _animatedAnimator.SetBool("isStrafeL",true);
         hips.AddForce(-hips.transform.right * strafeSpeed);
      } else
      {
         _animatedAnimator.SetBool("isStrafeL",false);
      }
      if (Input.GetKey(KeyCode.S))
      {
         _animatedAnimator.SetBool("isWalkBack",true);
         hips.AddForce(-hips.transform.forward * speed);
      }
      else if (!Input.GetKey(KeyCode.W))
      {
         _animatedAnimator.SetBool("isWalkBack", false);
      }

      if (Input.GetKey(KeyCode.D))
      {
         _animatedAnimator.SetBool("isStrafeR",true);
         hips.AddForce(hips.transform.right * strafeSpeed);
      }else
      {
         _animatedAnimator.SetBool("isStrafeR",false);
      }
      
      if (Input.GetAxis("Jump") > 0)
      {
         if (isGrounded)
         {
            hips.AddForce(new Vector3(0,jumpForce,0));
            isGrounded = false;
         }

         if (!isGrounded)
         {
            
         }
      }
      
   }
   public Vector2 FindVelRelativeToLook() {
      float lookAngle = orientation.transform.eulerAngles.y;
      float moveAngle = Mathf.Atan2(hips.velocity.x, hips.velocity.z) * Mathf.Rad2Deg;

      float u = Mathf.DeltaAngle(lookAngle, moveAngle);
      float v = 90 - u;

      float magnitue = hips.velocity.magnitude;
      float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
      float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);
        
      return new Vector2(xMag, yMag);
   }
  
   
   
   
  
  

   

}

  

   
   
   
            

   

