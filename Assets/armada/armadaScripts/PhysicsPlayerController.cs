using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPlayerController : MonoBehaviour
{
    public float walkForce = 500f;
    public float runMultiplier = 2f;
    public float jumpForce = 700f;
    public Transform leftLegTarget;
    public Transform rightLegTarget;
    public Animator animator;

    private Rigidbody rb;
    private bool isGrounded;
    private Transform leftLeg, rightLeg;
    private float footOffset = 0.1f; // Offset to keep feet slightly above the ground

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        leftLeg = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        rightLeg = animator.GetBoneTransform(HumanBodyBones.RightFoot);
    }

    void FixedUpdate()
    {
        Move();
        Jump();
        ApplyIK();
    }

    void Move()
    {
        float moveDirectionX = Input.GetAxis("Horizontal");
        float moveDirectionZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(moveDirectionX, 0, moveDirectionZ).normalized;

        float force = Input.GetKey(KeyCode.LeftShift) ? walkForce * runMultiplier : walkForce;

        rb.AddForce(moveDirection * force * Time.deltaTime);

        // Apply forces to the legs to simulate walking
        ApplyLegForce(leftLeg, moveDirection);
        ApplyLegForce(rightLeg, moveDirection);
    }

    void Jump()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void ApplyLegForce(Transform leg, Vector3 moveDirection)
    {
        Rigidbody legRb = leg.GetComponent<Rigidbody>();
        if (legRb != null)
        {
            legRb.AddForce(moveDirection * walkForce * Time.deltaTime);
        }
    }

    void ApplyIK()
    {
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
        animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftLegTarget.position + Vector3.up * footOffset);
        animator.SetIKPosition(AvatarIKGoal.RightFoot, rightLegTarget.position + Vector3.up * footOffset);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}

