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
    [SerializeField] float gunRange; // Ateþ etme mesafesi

    [Header("Gravity Gun")]
    [SerializeField] float gravityGunForce; // Gravity Gun kuvveti
    [SerializeField] VisualEffect muzzleVFX;

    WeaponManager weaponManager;
    Animator anim;

    Vector3 hitPoint;
    private void Start()
    {
        // Maksimum mermi sayýsýný anlýk mermi sayýsýyla eþitle
        magazineBullet = _bullet;
        startRot = transform.localRotation;
        weaponManager = GetComponentInParent<WeaponManager>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // Oyuncu ateþ edebilir mi kontrol et ve ateþle
        if (playerCanShoot)
        {
            Fire();
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

    void RecoilLower()
    {
        currentRecoilIndex--;
        if (currentRecoilIndex < 0) currentRecoilIndex = 0;
    }

    void Fire()
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
                    ShootRay(gunRange, bulletImpact);
                    if (Input.GetKeyDown(KeyCode.Mouse1)) GetComponent<RifleScope>().Scope();
                    break;

                case WeaponType.Pistol:
                    // Tabanca: Sol fare tuþuna basýldýðýnda ateþle
                    if (Input.GetKeyDown(KeyCode.Mouse0)) ShootRay(gunRange, bulletImpact);
                    break;

                case WeaponType.Knife:
                    // Býçak
                    if (Input.GetKeyDown(KeyCode.Mouse0)) Debug.Log("sol atak");
                    if (Input.GetKeyDown(KeyCode.Mouse1)) Debug.Log("sað atak");
                       break;
            }
        }
    }

    void ShootRay(float fireDistance, GameObject bulletImpact)
    {
        // Ateþ ediliyor mu ve yeniden yükleniyor mu kontrol et
        if (Input.GetKey(KeyCode.Mouse0) && !playerShoots && !reloading)
        {
            if (_bullet > 0)
            {
                // Ateþ etmeyi baþlat ve bir süre sonra durdur
                playerShoots = true;

                // Mermiyi azalt
                _bullet--;
                muzzleVFX.Play();

                Vector3 forward = cam.transform.forward;
                Vector3 spreadBullet = cam.transform.TransformDirection(weaponManager.recoilVectorList[currentRecoilIndex]);
                Vector3 randomSpread = new(Random.Range(-0.03f, 0.03f), Random.Range(0, 0.015f));
                Vector3 spreadRay = forward + spreadBullet + randomSpread;

                // Bir objeye isabet etti mi?
                if (Physics.Raycast(cam.transform.position, spreadRay, out RaycastHit hit, fireDistance, interactionLayer))
                {
                    Instantiate(bulletImpact, hit.point + spreadBullet + randomSpread, Quaternion.LookRotation(spreadRay));
                    hitPoint = hit.point;
                }

                // Silahýn rotasyonunu yayýlma açýsýna göre ayarla
                Vector3 quaRot = new(weaponManager.recoilVectorList[currentRecoilIndex].x * -10f, 0f, 
                    weaponManager.recoilVectorList[currentRecoilIndex].y * -1f );
                Quaternion spreadRotation = Quaternion.Euler(quaRot);
                transform.localRotation *= spreadRotation;
            }

            
            currentRecoilIndex++;
            StartCoroutine(BasicTimer("playerShoots", firingRate));
        }
        else
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, startRot, firingRate);
        }
    }

    void GravityGun()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            gravityGunForce += 2.5f * Time.deltaTime;
            gravityGunForce = Mathf.Clamp(gravityGunForce, 0f, 5f);
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            ShootRay(gunRange, bulletImpact);

            PlayerControllerF playerCont = GetComponentInParent<PlayerControllerF>();

            // Oyuncunun bakýþ yönünü al
            Transform playerTransform = playerCont.gameObject.transform;
            Vector3 lookDirection = playerTransform.forward;

            // Kuvvet yönünü belirle ve tersine çevir
            Vector3 direction = lookDirection * -1f;
            Debug.Log(direction);

            Rigidbody rb = playerCont.GetComponent<Rigidbody>();
            rb.AddForce(direction * gravityGunForce * 1000f * Time.deltaTime, ForceMode.Impulse);

            if (Mathf.Abs(direction.x) < 0.2f || Mathf.Abs(direction.z) < 0.2f)
            {
                rb.AddForce(Vector3.up * gravityGunForce * 1000f * Time.deltaTime, ForceMode.Impulse);
            }

            gravityGunForce = 0f;
        }
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
    }

    IEnumerator BasicTimer(string variableName, float timer)
    {
        // Belirtilen süre boyunca bekle ve ilgili deðiþkeni sýfýrla
        yield return new WaitForSeconds(timer);

        if (variableName == "playerShoots") { playerShoots = false; } 
    }
}
