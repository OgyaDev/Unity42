using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Cinemachine;
using Unity.VisualScripting;

public class NetworkPlayerTT : NetworkBehaviour, IPlayerLeft
{
    public static NetworkPlayer Local { get; set; }

    
    
    
    //Cinemachine
     CinemachineVirtualCamera cinemachineVirtualCamera;
     
    [SerializeField] Transform cam;

    [SerializeField] Transform leftFoot;
    [SerializeField] Transform rightFoot;

    [SerializeField] ProceduralLegsController proceduralLegs;

    [SerializeField] Rigidbody headRb;

    [SerializeField] float feetGroundCheckDist;

    ConfigurableJoint hipsCj;
    Rigidbody hipsRb;

    LayerMask groundMask;

    [SerializeField] float moveSpeed;
    [SerializeField] float rotationForce;
    [SerializeField] float balanceForce;
    [SerializeField] float jumpForce;
    [SerializeField] float speedMultiplier = 1.5f;
    [SerializeField] float maxVelocityChange;

    
    bool isJumpButtonPressed;
    bool isGrounded;
    bool isDead = false;

    Vector2 moveInputVector;

    [SerializeField] ConfigurableJoint[] cjs;
    JointDrive[] jds;
    JointDrive inAirDrive;
    JointDrive hipsInAirDrive;

    [SerializeField] float airSpring;
    private void Start()
    {
        jds = new JointDrive[cjs.Length];

        inAirDrive.maximumForce = Mathf.Infinity;
        inAirDrive.positionSpring = airSpring;

        hipsInAirDrive.maximumForce = Mathf.Infinity;
        hipsInAirDrive.positionSpring = 0;

        hipsRb = GetComponent<Rigidbody>();
        hipsCj = GetComponent<ConfigurableJoint>();

        //Saves the initial drives of each configurable joint
        for(int i = 0; i < cjs.Length; i++)
        {
            jds[i] = cjs[i].angularXDrive;
        }

        groundMask = LayerMask.GetMask("Ground");
        
    }
    void Update()
    {
        if (!isDead)
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
    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if (isGrounded && !isDead)
            {
                StabilizeBody();
                Move();
            }  
        }
    }
    
 //  if (GetInput(out NetworkInputData networkInputData))
 //  {
 //      
 //  }

    void SetPlayerInputs()
    {
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");
    }

    void StabilizeBody()
    {
        headRb.AddForce(Vector3.up * balanceForce);
        hipsRb.AddForce(Vector3.down * balanceForce);
    }

    void Move()
    {
        
    }
        //Zıplama
    void Jump()
    {
        hipsRb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        hipsRb.AddTorque(new Vector3(750, 0));
        isJumpButtonPressed = true;
    }
       //GroundCheck Raycast ile
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
        else if((!rightCheck && !leftCheck) && isGrounded)
        {
            Die(true);
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
        for(int i = 0; i < cjs.Length; i++)
        {
            cjs[i].angularXDrive = jds[i];
            cjs[i].angularYZDrive = jds[i];

        }

        proceduralLegs.EnableIk();
        isGrounded = true;
    }
   

    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();
        
        //Move DATA
        networkInputData.movementInput = moveInputVector;

        if (isJumpButtonPressed)
            networkInputData.isJumpPressed = true;
        
        //Reset Jump
        isJumpButtonPressed = false;

        return networkInputData;
    }
    
    
    
//  public override void Spawned()
//  {
//      if (Object.HasInputAuthority)
//      {
//          Local = this;

//          cinemachineVirtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
//          cinemachineVirtualCamera.m_Follow = transform;
//          cinemachineVirtualCamera.m_LookAt = transform;
//          
//          Utils.DebugLog("Spawned player with input authority");
//        
//          
//      }else
//        
//          Utils.DebugLog("Spawned player withOUT input authority");
//      //Hangi oyuncunun hangisi olduğunu anlamamıza yarayacak.
//      transform.name = $"P_{Object.Id}";
//  }

    public void PlayerLeft(PlayerRef player)
    {
        throw new System.NotImplementedException();
    }
}
