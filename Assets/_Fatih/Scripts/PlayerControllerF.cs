using UnityEngine;

public class PlayerControllerF : MonoBehaviour
{
    Rigidbody rb;
    public float speed;
    public float jumpForce;
    bool isJumping;
    public float horizontalMovement, verticalMovement;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        horizontalMovement = Input.GetAxis("Horizontal");
        verticalMovement = Input.GetAxis("Vertical");

        Vector3 movements = new(horizontalMovement * speed * Time.deltaTime, rb.velocity.y, verticalMovement * speed * Time.deltaTime);

        rb.velocity = movements;

        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            Vector3 jumpingMovement = new(rb.velocity.x, jumpForce);
            rb.velocity = jumpingMovement;
            isJumping = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Finish"))
        {
            isJumping = false;
        }
    }
}
