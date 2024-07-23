using UnityEngine;
using UnityEngine.UI;

public class PlayerControllerF : MonoBehaviour
{
    [Header("Player Features")]
    [SerializeField] float moveSpeed;
    [SerializeField] float crouchHeight;
    [SerializeField] float jumpForce;
    [SerializeField] float gravityForce;
    [SerializeField] bool isGrounded;
    [SerializeField] Transform groundChecker;
    [SerializeField] bool isCrouch;
    [SerializeField] bool isAttacking;

    [SerializeField] GameObject player;

    float horizontalMovement;
    float verticalMovement;
    Vector3 gravityV3;

    CharacterController charCont;

    private void Start()
    {
        charCont = GetComponent<CharacterController>();
    }

    private void Update()
    {
        PlayerMovements();
        Gravity();
    }

    void PlayerMovements()
    {
        //Movements WASD --------------------------------------------------------------------------------------------------- //
        horizontalMovement = Input.GetAxis("Horizontal");
        verticalMovement = Input.GetAxis("Vertical");

        Vector3 V3movement = (horizontalMovement * transform.right + verticalMovement * transform.forward).normalized;
        charCont.Move(moveSpeed * Time.deltaTime * V3movement);


        //Jump ------------------------------------------------------------------------------------------------------------- //
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            gravityV3.y = Mathf.Sqrt(jumpForce * -2f * gravityForce);
        }

        //Speed ------------------------------------------------------------------------------------------------------------- //
        if (isGrounded && !isCrouch)
        {
            if (horizontalMovement != 0 || verticalMovement != 0)
            {
                moveSpeed = 3f;
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    moveSpeed = 5f;
                }
            }
            else
            {
                moveSpeed = 0f;
            }
        }

    }

    void Gravity()
    {
        isGrounded = Physics.CheckSphere(groundChecker.position, 0.35f, LayerMask.GetMask("Ground"));

        gravityV3.y += gravityForce * Time.deltaTime;
        charCont.Move(gravityV3 * Time.deltaTime);

        if (gravityV3.y < 0 && isGrounded)
        {
            gravityV3.y = -3f;
        }
    }
}