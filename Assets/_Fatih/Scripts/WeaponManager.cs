using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] List<GameObject> weapons;

    PlayerControllerF _playerController;
    Rigidbody _playerRb;

    void Start()
    {
        _playerController = FindAnyObjectByType<PlayerControllerF>();
        _playerRb = _playerController.GetComponent<Rigidbody>();
    }

    void Update()
    {
        float MovementX = _playerController.horizontalMovement;
        float MovementY = _playerController.verticalMovement;
        float MovementJump = _playerRb.velocity.y; 
        MovementJump = Mathf.Clamp(MovementJump, -4f, 4f);

        foreach (var weapon in weapons)
        {
            weapon.transform.localRotation = Quaternion.Euler(MovementJump, MovementY * -3f, MovementX * 3f );
        }
    }


}
