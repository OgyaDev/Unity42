using System.Collections;
using UnityEngine;

public class WeaponMovements : MonoBehaviour
{
    [SerializeField] Transform movingPart; // Hareket edecek olan k�s�m
    [SerializeField] Transform donenmec; // Hareket edecek olan k�s�m
    [SerializeField] Vector3 targetPosition; // Hedef konum
    [SerializeField] float speed = 1.0f; // Hareket h�z�

    private Vector3 initialPosition;

    void Start()
    {
        // Ba�lang�� konumunu kaydedin
        initialPosition = movingPart.localPosition;
    }

    public void SlideMovement()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            StartCoroutine(MoveToTargetAndBack());
        }
    }

    public void RotatePortal()
    {
        donenmec.Rotate(0f,0f,5f);
    }

    IEnumerator MoveToTargetAndBack()
    {
        // Hedef konuma do�ru hareket
        while (Vector3.Distance(movingPart.localPosition, targetPosition) > 0.01f)
        {
            movingPart.localPosition = Vector3.MoveTowards(movingPart.localPosition, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        // Eski konuma geri hareket
        while (Vector3.Distance(movingPart.localPosition, initialPosition) > 0.01f)
        {
            movingPart.localPosition = Vector3.MoveTowards(movingPart.localPosition, initialPosition, speed * Time.deltaTime);
            yield return null;
        }

        // Tam olarak ba�lang�� konumuna yerle�tirme
        movingPart.localPosition = initialPosition;
    }
}
