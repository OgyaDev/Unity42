using UnityEngine;

public class CameraCont : MonoBehaviour
{
    public Transform player;
    public float mouseSensivity;
    float xRotation;
    public bool mouseActivity;

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!mouseActivity)
        {
            MouseInputs();
        }
    }

    void MouseInputs()
    {
        float mouseXPos = Input.GetAxis("Mouse X") * mouseSensivity * Time.deltaTime;
        float mouseYPos = Input.GetAxis("Mouse Y") * mouseSensivity * Time.deltaTime;

        xRotation -= mouseYPos;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        player.Rotate(Vector3.up * mouseXPos);
    }

}