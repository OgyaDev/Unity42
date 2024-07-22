using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class Weapon : MonoBehaviour
{
    // Silah türlerini belirten bir enum
    public enum WeaponType { Knife, Pistol, Rifle, GravityGun };
    public WeaponType weaponType;

    Quaternion startRot;

    [SerializeField] LayerMask interactionLayer; // Etkileþim katmaný
    [SerializeField] GameObject bulletImpact; // Kurþun çarptýðýnda kullanýlacak efekt
    [SerializeField] GameObject hitEffect; // Kurþun çarptýðýnda kullanýlacak efekt
    [SerializeField] Camera cam; // Silahýn kullanacaðý kamera

    [Header("General Specs")]
    [SerializeField] int Damage; // Silahýn verdiði hasar
    [SerializeField] int spareBullet; // Yedek mermi sayýsý
    [SerializeField] int magazineBullet; // Silahýn þarjörü doluyken alabileceði maksimum mermi sayýsý
    [SerializeField] int _bullet; // Anlýk mermi sayýsý
    [SerializeField] int currentRecoilIndex;
    [SerializeField] bool reloading; // Yeniden yükleme yapýlýyor mu?
    [SerializeField] bool playerShoots; // Oyuncu þu anda ateþ ediyor mu?
    [SerializeField] bool playerCanShoot = true; // Oyuncu ateþ edebilir mi?
    [SerializeField] float firingRate; // Ateþ etme hýzý
    [SerializeField] float fireDistance; // Ateþ etme mesafesi

    [Header("Gravity Gun")]
    [SerializeField] float gravityGunForce; // Gravity Gun kuvveti
    [SerializeField] VisualEffect muzzleVFX;

    WeaponManager weaponManager;
    Animator anim;

    private void OnEnable()
    {
        anim = GetComponent<Animator>();

        if (_bullet <= 0)
        {
            reloading = true;
            anim.SetTrigger("Reload");
        }
        else
        {
            anim.SetTrigger("HoldingGun");
            playerCanShoot = false;
        }
    }

    private void Start()
    {
        // Maksimum mermi sayýsýný anlýk mermi sayýsýyla eþitle
        magazineBullet = _bullet;
        startRot = transform.localRotation;
        weaponManager = GetComponentInParent<WeaponManager>();
    }

    private void Update()
    {
        // Oyuncu ateþ edebilir mi kontrol et ve ateþle
        if (playerCanShoot)
        {
            SwitchWeapon();
        }

        // R tuþuna basýldýðýnda ve mermiler bittiðinde yeniden yükle
        if (_bullet != magazineBullet && !reloading && spareBullet > 0 && (Input.GetKeyDown(KeyCode.R) || _bullet <= 0))
        {
            reloading = true;
            anim.SetTrigger("Reload");
        }

        if (currentRecoilIndex > 0 && !playerShoots)
        {
            Invoke(nameof(RecoilLower), firingRate * 2);
        }

    }

    private void LateUpdate()
    {
        if (Input.GetKey(KeyCode.Mouse0) && playerShoots && !reloading)
        {
            if (_bullet > 0)
            {
                {
                    // Silahýn rotasyonunu yayýlma açýsýna göre ayarla
                    Vector3 quaRot = new(0f, weaponManager.recoilVectorList[currentRecoilIndex].x * 100f,
                        weaponManager.recoilVectorList[currentRecoilIndex].y * -100f);
                    Quaternion spreadRotation = Quaternion.Euler(quaRot);

                    transform.localRotation = Quaternion.Slerp(transform.localRotation, transform.localRotation * spreadRotation, firingRate * 2);
                }
            }
        }
        else
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, startRot, firingRate);
        }
    }

    void RecoilLower()
    {
        currentRecoilIndex--;
        if (currentRecoilIndex < 0) currentRecoilIndex = 0;
    }

    void SwitchWeapon()
    {
        // Ateþ etmiyorsa ve yeniden yüklenmiyorsa ateþle
        if (!playerShoots && !reloading)
        {
            switch (weaponType)
            {
                case WeaponType.GravityGun:
                    // Gravity Gun: Sað fare tuþuna basarak kuvveti artýr, býrakýldýðýnda ateþle
                    // if (Input.GetKeyDown(KeyCode.Mouse0)) Fire();
                    GravityGun();
                    break;

                case WeaponType.Rifle:
                    // Makinalý Tüfek: Sürekli ateþle
                    Fire();
                    if (Input.GetMouseButtonDown(1)) GetComponent<RifleScope>().Scope();
                    break;

                case WeaponType.Pistol:
                    // Tabanca: Sol fare tuþuna basýldýðýnda ateþle
                    if (Input.GetMouseButtonDown(0)) Fire();
                    break;

                case WeaponType.Knife:
                    // Býçak
                    KnifeAttacks();
                    break;
            }
        }
    }

    void Fire()
    {
        // Ateþ ediliyor mu ve yeniden yükleniyor mu kontrol et
        if (Input.GetMouseButton(0) && !playerShoots && !reloading)
        {
            if (_bullet > 0)
            {
                // Mermiyi azalt
                _bullet--;
                muzzleVFX.Play();

                ShootRay();
            }

            // Ateþ etmeyi baþlat ve bir süre sonra durdur
            playerShoots = true;
            currentRecoilIndex++;
            StartCoroutine(BasicTimer("playerShoots", firingRate));
        }
    }

    void ShootRay()
    {
        Vector3 forward = cam.transform.forward;
        Vector3 randomSpread = new(Random.Range(-0.03f, 0.03f), Random.Range(0, 0.015f));
        Vector3 spreadBullet = cam.transform.TransformDirection(weaponManager.recoilVectorList[currentRecoilIndex]);
        Vector3 spreadRay = forward + spreadBullet + randomSpread;

        // Bir objeye isabet etti mi?
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, forward, out hit, fireDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Instantiate(hitEffect, hit.point + spreadBullet + randomSpread, Quaternion.LookRotation(spreadRay));
            }
            else
            {
                Instantiate(bulletImpact, hit.point + spreadBullet + randomSpread, Quaternion.LookRotation(spreadRay));
            }
        }
    }

    void GravityGun()
    {
        Vector3 forward = cam.transform.forward;
        RaycastHit hit;

        //if (Input.GetMouseButtonDown(0))
        //{
        //    if (Physics.Raycast(cam.transform.position, forward, out hit, fireDistance))
        //    {
        //        if (hit.collider.CompareTag("Player"))
        //        {
        //            Instantiate(hitEffect, hit.point, Quaternion.identity);
        //        }
        //        else
        //        {
        //            Instantiate(bulletImpact, hit.point, Quaternion.identity);
        //        }
        //    }
        //}

        if (Input.GetMouseButton(1))
        {
            gravityGunForce += 2.5f * Time.deltaTime;
            gravityGunForce = Mathf.Clamp(gravityGunForce, 0f, 5f);
        }
        if (Input.GetMouseButtonUp(1) && gravityGunForce >= 2f)
        {
            PlayerControllerF playerCont = GetComponentInParent<PlayerControllerF>();

            // Oyuncunun bakýþ yönünü al
            Transform playerTransform = playerCont.gameObject.transform;
            Vector3 lookDirection = playerTransform.forward;

            // Kuvvet yönünü belirle ve tersine çevir
            Vector3 direction = lookDirection * -1f;

            Rigidbody rb = playerCont.GetComponent<Rigidbody>();
            rb.AddForce(Vector3.up * gravityGunForce * 1500f * Time.deltaTime, ForceMode.Impulse);
            rb.AddForce(direction * gravityGunForce * 750f * Time.deltaTime, ForceMode.Impulse);

            if (Physics.Raycast(cam.transform.position, forward, out hit, fireDistance))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Instantiate(hitEffect, hit.point, Quaternion.identity);
                }
                else
                {
                    Instantiate(bulletImpact, hit.point, Quaternion.identity);
                }
            }

            gravityGunForce = 0f;
        }
    }

    void KnifeAttacks()
    {

    }

    public void Reload()
    {
        // Eksik mermileri hesapla ve yeniden yükle
        int missingBullets = magazineBullet - _bullet;
        int bulletsToLoad = Mathf.Min(spareBullet, missingBullets);

        _bullet += bulletsToLoad;
        spareBullet -= bulletsToLoad;

        // Mermi sayýlarýný sýnýrla
        _bullet = Mathf.Clamp(_bullet, 0, magazineBullet);
        spareBullet = Mathf.Clamp(spareBullet, 0, 100);

        reloading = false;
        playerCanShoot = true;
    }

    public void HoldingGun()
    {
        playerCanShoot = true;
    }

    IEnumerator BasicTimer(string variableName, float timer)
    {
        // Belirtilen süre boyunca bekle ve ilgili deðiþkeni sýfýrla
        yield return new WaitForSeconds(timer);

        if (variableName == "playerShoots") { playerShoots = false; }
    }

    private void OnDisable()
    {
        reloading = false;
        playerShoots = false;
        playerCanShoot = true;
    }
}
