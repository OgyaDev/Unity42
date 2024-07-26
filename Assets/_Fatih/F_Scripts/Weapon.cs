using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
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
    [SerializeField] GameObject magazineOnGun, leftHandMagazine;
    [SerializeField] Camera cam; // Silahýn kullanacaðý kamera
    [SerializeField] VisualEffect muzzleVFX;

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
    [SerializeField] GameObject[] laserBeam;

    WeaponManager weaponManager;
    Animator anim;
    WeaponMovements movementWeapon;

    private void OnEnable()
    {
        anim = GetComponent<Animator>();

        if (_bullet <= 0 && spareBullet > 0)
        {
            reloading = true;
            anim.SetTrigger("Reload");
        }
        else
        {
            anim.SetTrigger("HoldingGun");
            playerCanShoot = false;
        }
        weaponManager.TextBullentCount(_bullet, spareBullet);
    }

    private void Start()
    {
        // Maksimum mermi sayýsýný anlýk mermi sayýsýyla eþitle
        magazineBullet = _bullet;
        startRot = transform.localRotation;
        weaponManager = GetComponentInParent<WeaponManager>();
        movementWeapon = GetComponent<WeaponMovements>();
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
            leftHandMagazine.SetActive(true);
            magazineOnGun.SetActive(false);
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
                weaponManager.TextBullentCount(_bullet, spareBullet);
                muzzleVFX.Play();

                ShootRay();
                movementWeapon.SlideMovement();
            }

            // Ateþ etmeyi baþlat ve bir süre sonra durdur
            playerShoots = true;
            currentRecoilIndex++;
            StartCoroutine(BasicTimer());
        }
    }

    void ShootRay()
    {
        Vector3 forward = cam.transform.forward;
        Vector3 randomSpread = new(Random.Range(-0.03f, 0.03f), 0f);
        Vector3 spreadBullet = cam.transform.TransformDirection(weaponManager.recoilVectorList[currentRecoilIndex]);

        Vector3 spreadRay = forward + spreadBullet;
        if (currentRecoilIndex != 0) spreadRay.x += randomSpread.x;

        // Bir objeye isabet etti mi?
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, spreadRay, out hit, fireDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Instantiate(hitEffect, hit.point + spreadBullet + randomSpread, Quaternion.LookRotation(hit.normal));
            }
            else
            {
                Instantiate(bulletImpact, hit.point + spreadBullet + randomSpread, Quaternion.LookRotation(hit.normal));
            }
        }
    }

    void GravityGun()
    {
        Vector3 forward = cam.transform.forward;
        RaycastHit hit;

        if (Input.GetMouseButtonDown(0) && !playerShoots)
        {
            playerShoots = true;
            StartCoroutine(BasicTimer());

            if (Physics.Raycast(cam.transform.position, forward, out hit, fireDistance))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                }
                else
                {
                    Instantiate(bulletImpact, hit.point, Quaternion.LookRotation(hit.normal));
                }
            }
            movementWeapon.SlideMovement();
        }

        if (Input.GetMouseButton(1))
        {
            gravityGunForce += 2.5f * Time.deltaTime;
            gravityGunForce = Mathf.Clamp(gravityGunForce, 0f, 5f);
            LaserBeam(true);
            movementWeapon.RotatePortal();
        }
        if (Input.GetMouseButtonUp(1) && gravityGunForce >= 2f)
        {
            PlayerControllerF playerCont = GetComponentInParent<PlayerControllerF>();

            // Oyuncunun bakýþ yönünü al
            Transform playerTransform = playerCont.gameObject.transform;
            Vector3 lookDirection = playerTransform.forward;

            // Kuvvet yönünü belirle ve tersine çevir
            Vector3 direction = lookDirection * -1f;
            

            if (Physics.Raycast(cam.transform.position, forward, out hit, fireDistance))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                }
                else
                {
                    Instantiate(bulletImpact, hit.point, Quaternion.LookRotation(hit.normal));

                    Rigidbody rb = playerCont.GetComponent<Rigidbody>();
                    rb.AddForce(Vector3.up * gravityGunForce * 1000f * Time.deltaTime, ForceMode.Impulse);
                    rb.AddForce(direction * gravityGunForce * 500f * Time.deltaTime, ForceMode.Impulse);
                }
            }

            LaserBeam(false);
            gravityGunForce = 0f;
        }
    }


    void LaserBeam(bool isActive)
    {
        if (isActive)
        {
            laserBeam[0].SetActive(true);
            Vector3 maxScale = new Vector3(0.075f, 0.075f, 0.075f);
            Vector3 minScale = new Vector3(0.01f, 0.01f, 0.01f);

            foreach (var efx in laserBeam)
            {
                Vector3 currentScale = efx.transform.localScale;

                currentScale += new Vector3(0.03f, 0.03f, 0.03f) * Time.deltaTime;

                currentScale.x = Mathf.Clamp(currentScale.x, minScale.x, maxScale.x);
                currentScale.y = Mathf.Clamp(currentScale.y, minScale.y, maxScale.y);
                currentScale.z = Mathf.Clamp(currentScale.z, minScale.z, maxScale.z);

                efx.transform.localScale = currentScale;
            }
        }
        else
        {
            laserBeam[0].SetActive(false);
            foreach (var efx in laserBeam) { efx.transform.localScale = new(0.01f, 0.01f, 0.01f); }
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
        weaponManager.TextBullentCount(_bullet, spareBullet);

        leftHandMagazine.SetActive(false);
        magazineOnGun.SetActive(true);
    }

    public void HoldingGun()
    {
        playerCanShoot = true;
    }

    IEnumerator BasicTimer()
    {
        // Belirtilen süre boyunca bekle ve ilgili deðiþkeni sýfýrla
        yield return new WaitForSeconds(firingRate);
        playerShoots = false; 
    }

    private void OnDisable()
    {
        reloading = false;
        playerShoots = false;
        playerCanShoot = true;
    }
}
