using UnityEngine;

public class CollectibleWeapon : MonoBehaviour
{
    [SerializeField] GameObject machineGun;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            machineGun.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
