using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] List<GameObject> weapons;
    public List<Vector3> recoilVectorList;
    

    public bool haveRifle;
    public bool haveGravityGun;

    Dictionary<KeyCode, int> keyToWeapon;

    private void Start()
    {
        keyToWeapon = new Dictionary<KeyCode, int>
        {
            { KeyCode.Alpha1, 2 }, // Rifle
            { KeyCode.Alpha2, 1 }, // Pistol
            { KeyCode.Alpha3, 0 }, // Knife
            { KeyCode.Alpha4, 3 }  // Gravity Gun
        };

        if (haveRifle)
        {
            foreach (var weapon in weapons) { weapon.SetActive(false); }
            weapons[2].SetActive(true);
        }
        else
        {
            foreach (var weapon in weapons) { weapon.SetActive(false); }
            weapons[1].SetActive(true);
        }
        
    }

    private void Update()
    {
        SwitchWeapon();
    }

    void SwitchWeapon()
    {
        foreach (var key in keyToWeapon.Keys)
        {
            if (Input.GetKeyDown(key))
            {
                int weaponIndex = keyToWeapon[key];

                if (weaponIndex == 2 && !haveRifle) continue;
                if (weaponIndex == 3 && !haveGravityGun) continue;

                foreach (var weapon in weapons)
                {
                    weapon.SetActive(false);
                }
                weapons[weaponIndex].SetActive(true);
                break;
            }
        }
    }
}
