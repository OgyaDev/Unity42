using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerControllerWIK : MonoBehaviour
{
    public float moveForce = 500f;
    public float runMultiplier = 2f;
    public float jumpForce = 700f;

    [SerializeField] Rigidbody hipRb;
    private bool isGrounded;

    public CCDIKSolver leftLegIK;
    public CCDIKSolver rightLegIK;

    public Transform leftFootTarget;
    public Transform rightFootTarget;

    // PD Controller parameters
    public float balanceTorque = 500f;
    public float balanceDamping = 0.5f;

    void Start()
    {
        hipRb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Move();
        Jump();
        Balance();
    }

    void LateUpdate()
    {
        ApplyIK();
    }

    void Move()
    {
        float moveDirectionX = Input.GetAxis("Horizontal");
        float moveDirectionZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(moveDirectionX, 0, moveDirectionZ).normalized;

        float force = Input.GetKey(KeyCode.LeftShift) ? moveForce * runMultiplier : moveForce;

        // Apply force to the hip to move the character
        hipRb.AddForce(moveDirection * force * Time.deltaTime);
    }

    void Jump()
    {
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            hipRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void Balance()
    {
        // Calculate the desired upright rotation
        Quaternion desiredRotation = Quaternion.Euler(0, hipRb.rotation.eulerAngles.y, 0);

        // Calculate the error in rotation
        Quaternion rotationError = desiredRotation * Quaternion.Inverse(hipRb.rotation);

        // Convert the rotation error to a vector3 torque
        Vector3 torqueError = new Vector3(rotationError.x, rotationError.y, rotationError.z) * balanceTorque;

        // Apply damping
        Vector3 damping = hipRb.angularVelocity * balanceDamping;

        // Apply the balance torque
        hipRb.AddTorque(torqueError - damping);
    }

    void ApplyIK()
    {
        leftLegIK.target.position = leftFootTarget.position;
        rightLegIK.target.position = rightFootTarget.position;
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

