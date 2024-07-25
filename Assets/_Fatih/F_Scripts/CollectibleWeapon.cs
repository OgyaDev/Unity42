using UnityEngine;

public class CollectibleWeapon : MonoBehaviour
{
    [SerializeField] bool isRifle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            WeaponManager weaponManager = other.GetComponentInChildren<WeaponManager>();

            weaponManager.CollectWeapon(isRifle);
            this.gameObject.SetActive(false);
        }
    }
}
