using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class Weapon : MonoBehaviour
{
    // Silah türlerini belirten bir enum
    public enum WeaponType { Knife, Pistol, MachineGun, GravityGun };
    public WeaponType weaponType;

    [SerializeField] LayerMask interactionLayer; // Etkileþim katmaný
    [SerializeField] GameObject bulletImpact; // Kurþun çarptýðýnda kullanýlacak efekt
    [SerializeField] GameObject Player; // Kurþun çarptýðýnda kullanýlacak efekt
    [SerializeField] Camera cam; // Silahýn kullanacaðý kamera

    [Header("General Specs")]
    [SerializeField] int Damage; // Silahýn verdiði hasar
    [SerializeField] int spareBullet; // Yedek mermi sayýsý
    [SerializeField] int magazineBullet; // Silahýn þarjörü doluyken alabileceði maksimum mermi sayýsý
    [SerializeField] int _bullet; // Anlýk mermi sayýsý
    [SerializeField] bool reloading; // Yeniden yükleme yapýlýyor mu?
    [SerializeField] bool playerShoots; // Oyuncu þu anda ateþ ediyor mu?
    [SerializeField] bool playerCanShoot = true; // Oyuncu ateþ edebilir mi?
    [SerializeField] float spreadAngle = 5f; // Merminin yayýlma açýsý
    [SerializeField] float firingRate; // Ateþ etme hýzý
    [SerializeField] float gunRange; // Ateþ etme mesafesi

    [Header("Gravity Gun")]
    [SerializeField] float gravityGunForce; // Gravity Gun kuvveti
    [SerializeField] VisualEffect muzzleVFX;

    void OnEnable()
    {
        // Maksimum mermi sayýsýný anlýk mermi sayýsýyla eþitle
        magazineBullet = _bullet;
    }

    private void Update()
    {
        // Oyuncu ateþ edebilir mi kontrol et ve ateþle
        if (playerCanShoot)
        {
            Fire();
        }

        // R tuþuna basýldýðýnda ve mermiler bittiðinde yeniden yükle
        if (_bullet != magazineBullet && (Input.GetKeyDown(KeyCode.R) && !reloading && spareBullet > 0 || _bullet <= 0 && spareBullet > 0 && !reloading))
        {
            Reload();
        }
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
                    if (Input.GetKey(KeyCode.Mouse1))
                    {
                        gravityGunForce += 2.5f * Time.deltaTime;
                        gravityGunForce = Mathf.Clamp(gravityGunForce, 0f, 5f);
                    }
                    if (Input.GetKeyUp(KeyCode.Mouse1))
                    {
                        ShootRay(gunRange, bulletImpact);
                        Rigidbody rb = FindAnyObjectByType<PlayerControllerF>().GetComponent<Rigidbody>();
                        rb.AddForce(Vector3.up * gravityGunForce * gunRange * Time.deltaTime, ForceMode.Impulse);
                        gravityGunForce = 0f;
                    }
                    break;

                case WeaponType.MachineGun:
                    // Makinalý Tüfek: Sürekli ateþle
                    ShootRay(gunRange, bulletImpact);
                    break;

                case WeaponType.Pistol:
                    // Tabanca: Sol fare tuþuna basýldýðýnda ateþle
                    if (Input.GetKeyDown(KeyCode.Mouse0)) ShootRay(gunRange, bulletImpact);
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
                // Mermiyi azalt
                _bullet--;
                muzzleVFX.Play();

                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Bir objeye isabet etti mi?
                if (Physics.Raycast(ray, out hit, fireDistance, interactionLayer))
                {
                    // Kurþunun yayýlma açýsýný hesapla ve mermi etkisini oluþtur
                    float spreadX = Random.Range(-spreadAngle, spreadAngle);
                    float spreadY = Random.Range(0f, spreadAngle);

                    Vector3 spreadPosition = new(spreadX, spreadY, spreadX);
                    Instantiate(bulletImpact, hit.point + spreadPosition, Quaternion.identity);

                    // Silahýn rotasyonunu yayýlma açýsýna göre ayarla
                    Quaternion spreadRotation = Quaternion.Euler(spreadX, spreadY, 0);
                    transform.rotation = spreadRotation;
                }
            }

            // Ateþ etmeyi baþlat ve bir süre sonra durdur
            playerShoots = true;
            StartCoroutine(BasicTimer("playerShoots", firingRate));
        }
    }

    void Reload()
    {
        // Eksik mermileri hesapla ve yeniden yükle
        int missingBullets = magazineBullet - _bullet;
        int bulletsToLoad = Mathf.Min(spareBullet, missingBullets);

        _bullet += bulletsToLoad;
        spareBullet -= bulletsToLoad;

        // Mermi sayýlarýný sýnýrla
        _bullet = Mathf.Clamp(_bullet, 0, magazineBullet);
        spareBullet = Mathf.Clamp(spareBullet, 0, 100);

        // Yeniden yüklemeyi baþlat
        reloading = true;
        StartCoroutine(BasicTimer("reloading", 3f));
    }

    IEnumerator BasicTimer(string variableName, float timer)
    {
        // Belirtilen süre boyunca bekle ve ilgili deðiþkeni sýfýrla
        yield return new WaitForSeconds(timer);

        if (variableName == "playerShoots") { playerShoots = false; }
        if (variableName == "reloading") { reloading = false; }
    }
}
