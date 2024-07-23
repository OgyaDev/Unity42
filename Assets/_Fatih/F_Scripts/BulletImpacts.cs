using UnityEngine;

public class BulletImpacts : MonoBehaviour
{
    [SerializeField] float destroyTime;

    private void OnEnable()
    {
        Destroy(gameObject, destroyTime);
    }
}
