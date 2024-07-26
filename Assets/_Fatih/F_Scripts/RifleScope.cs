using UnityEngine;

public class RifleScope : MonoBehaviour
{
    [SerializeField] Transform handPosition;
    [SerializeField] Transform scopePosition;

    bool scopeIsOpened;

    public void Scope()
    {
        scopeIsOpened = !scopeIsOpened;

        if (scopeIsOpened)
        {
            transform.position = scopePosition.position;
        }
        else
        {
            transform.position = handPosition.position;
        }
    }

    private void OnDisable()
    {
        scopeIsOpened = true;
        Scope();
    }

}
