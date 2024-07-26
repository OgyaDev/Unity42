using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class PlayerController2 : NetworkBehaviour
{
    [SerializeField] Transform cam;

    [SerializeField] Transform leftFoot;
    [SerializeField] Transform rightFoot;

    [SerializeField] ProceduralLegsController proceduralLegs;

    [SerializeField] Rigidbody headRb;

    [SerializeField] float feetGroundCheckDist;

    [SerializeField] ConfigurableJoint hipsCj;
    [SerializeField] Rigidbody hipsRb;

    LayerMask groundMask;

    [SerializeField] float moveSpeed;
    [SerializeField] float rotationForce;
    [SerializeField] float balanceForce;
    [SerializeField] float jumpForce;
    [SerializeField] float speedMultiplier = 1.5f;
    [SerializeField] float maxVelocityChange;
   
    public Transform torso;
    public float mouseX;
    public float mouseY;
    public float rotationSpeed = 5f;
    private float yaw = 0f;
    private float pitch = 0f;
    
    bool isGrounded;
    bool isDead = false;

    float horizontal, vertical;

    [SerializeField] ConfigurableJoint[] cjs;
    JointDrive[] jds;
    JointDrive inAirDrive;
    JointDrive hipsInAirDrive;

    [SerializeField] float airSpring;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        InitializeComponents();
    }
   
    private void Start()
    {
        if (IsOwner)
        {
            InitializeComponents();
        }
    }

    void InitializeComponents()
    {
        jds = new JointDrive[cjs.Length];

        inAirDrive.maximumForce = Mathf.Infinity;
        inAirDrive.positionSpring = airSpring;

        hipsInAirDrive.maximumForce = Mathf.Infinity;
        hipsInAirDrive.positionSpring = 0;

        if (hipsRb == null)
        {
            hipsRb = GetComponent<Rigidbody>();
        }

        if (hipsCj == null)
        {
            hipsCj = GetComponent<ConfigurableJoint>();
        }

        for (int i = 0; i < cjs.Length; i++)
        {
            jds[i] = cjs[i].angularXDrive;
        }

        groundMask = LayerMask.GetMask("Ground");
    }

    void SetPlayerInputs()
    {
        // Get mouse input
        mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.V))
            StabilizeBody();
        if (!isDead && IsOwner)
        {
            proceduralLegs.GroundHomeParent();
            CheckGrounded();
            SetPlayerInputs();

            if (isGrounded)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                    Jump();
            }
        }
    }

    void FixedUpdate()
    {
        if (isGrounded && !isDead && IsOwner)
        {
            StabilizeBody();
            Move();
        }
    }
    
   void Move()
{
    // Update yaw and pitch
    yaw += mouseX * rotationSpeed;
    pitch -= mouseY * rotationSpeed;
    pitch = Mathf.Clamp(pitch, -90f, 90f);

    // Rotate torso based on mouse input
    torso.localRotation = Quaternion.Euler(pitch, yaw, 0f);

    // Calculate movement direction based on camera's forward direction
    Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
    Vector3 rotatedMoveDirection = cam.TransformDirection(moveDirection);
    rotatedMoveDirection.y = 0f; // Keep movement strictly horizontal

    // If there's any movement input
    if (moveDirection.magnitude > 0.1f)
    {
        // Calculate target rotation
        Quaternion targetRotation = Quaternion.LookRotation(rotatedMoveDirection);

        // Rotate the character smoothly towards the target rotation
        hipsRb.MoveRotation(Quaternion.RotateTowards(hipsRb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

        // Calculate and apply movement force
        Vector3 targetVelocity = rotatedMoveDirection * moveSpeed;
        Vector3 velocityChange = targetVelocity - hipsRb.velocity;
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0; // Prevent vertical velocity change

        hipsRb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    // Apply balance forces
    headRb.AddForce(Vector3.up * balanceForce);
    hipsRb.AddForce(Vector3.down * balanceForce);

    // Jump logic
    if (isGrounded && !isDead && Input.GetKeyDown(KeyCode.Space))
    {
        hipsRb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        hipsRb.AddTorque(new Vector3(750, 0));
    }

    // Adjust speed if running
    if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift))
    {
        Vector3 runVelocity = rotatedMoveDirection * (moveSpeed * speedMultiplier);
        Vector3 runVelocityChange = runVelocity - hipsRb.velocity;
        runVelocityChange.x = Mathf.Clamp(runVelocityChange.x, -maxVelocityChange, maxVelocityChange);
        runVelocityChange.z = Mathf.Clamp(runVelocityChange.z, -maxVelocityChange, maxVelocityChange);
        runVelocityChange.y = 0;

        hipsRb.AddForce(runVelocityChange, ForceMode.VelocityChange);
    }
}



    void StabilizeBody()
    {
        headRb.AddForce(Vector3.up * balanceForce);
        hipsRb.AddForce(Vector3.down * balanceForce);
    }

    void CheckGrounded()
    {
        bool leftCheck = false;
        bool rightCheck = false;
        RaycastHit hit;

        if (Physics.Raycast(leftFoot.position, Vector3.down, out hit, feetGroundCheckDist, groundMask))
            leftCheck = true;

        if (Physics.Raycast(rightFoot.position, Vector3.down, out hit, feetGroundCheckDist, groundMask))
            rightCheck = true;

        if ((rightCheck || leftCheck) && !isGrounded)
        {
            SetDrives();
        }
     
    }

    public void Die(bool respawn)
    {
        foreach (ConfigurableJoint cj in cjs)
        {
            cj.angularXDrive = inAirDrive;
            cj.angularYZDrive = inAirDrive;
        }

        hipsCj.angularYZDrive = hipsInAirDrive;
        hipsCj.angularXDrive = hipsInAirDrive;

        proceduralLegs.DisableIk();
        isGrounded = false;

        if (!respawn)
            isDead = true;
    }

    void SetDrives()
    {
        for (int i = 0; i < cjs.Length; i++)
        {
            cjs[i].angularXDrive = jds[i];
            cjs[i].angularYZDrive = jds[i];
        }

        proceduralLegs.EnableIk();
        isGrounded = true;
    }

    void Jump()
    {
        hipsRb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        hipsRb.AddTorque(new Vector3(750, 0));
    }
}

