using UnityEngine;

public class CollectibleWeapon : MonoBehaviour
{
    [SerializeField] bool isRifle;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WeaponManager weaponManager = other.GetComponentInChildren<WeaponManager>();

            weaponManager.CollectWeapon(isRifle);
            gameObject.SetActive(false);
        }
    }
}
