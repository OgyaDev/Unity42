using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class Weapon : MonoBehaviour
{
    // Silah t�rlerini belirten bir enum
    public enum WeaponType { Knife, Pistol, MachineGun, GravityGun };
    public WeaponType weaponType;

    [SerializeField] LayerMask interactionLayer; // Etkile�im katman�
    [SerializeField] GameObject bulletImpact; // Kur�un �arpt���nda kullan�lacak efekt
    [SerializeField] GameObject Player; // Kur�un �arpt���nda kullan�lacak efekt
    [SerializeField] Camera cam; // Silah�n kullanaca�� kamera

    [Header("General Specs")]
    [SerializeField] int Damage; // Silah�n verdi�i hasar
    [SerializeField] int spareBullet; // Yedek mermi say�s�
    [SerializeField] int magazineBullet; // Silah�n �arj�r� doluyken alabilece�i maksimum mermi say�s�
    [SerializeField] int _bullet; // Anl�k mermi say�s�
    [SerializeField] bool reloading; // Yeniden y�kleme yap�l�yor mu?
    [SerializeField] bool playerShoots; // Oyuncu �u anda ate� ediyor mu?
    [SerializeField] bool playerCanShoot = true; // Oyuncu ate� edebilir mi?
    [SerializeField] float spreadAngle = 5f; // Merminin yay�lma a��s�
    [SerializeField] float firingRate; // Ate� etme h�z�
    [SerializeField] float gunRange; // Ate� etme mesafesi

    [Header("Gravity Gun")]
    [SerializeField] float gravityGunForce; // Gravity Gun kuvveti
    [SerializeField] VisualEffect muzzleVFX;

    void OnEnable()
    {
        // Maksimum mermi say�s�n� anl�k mermi say�s�yla e�itle
        magazineBullet = _bullet;
    }

    private void Update()
    {
        // Oyuncu ate� edebilir mi kontrol et ve ate�le
        if (playerCanShoot)
        {
            Fire();
        }

        // R tu�una bas�ld���nda ve mermiler bitti�inde yeniden y�kle
        if (_bullet != magazineBullet && (Input.GetKeyDown(KeyCode.R) && !reloading && spareBullet > 0 || _bullet <= 0 && spareBullet > 0 && !reloading))
        {
            Reload();
        }
    }

    void Fire()
    {
        // Ate� etmiyorsa ve yeniden y�klenmiyorsa ate�le
        if (!playerShoots && !reloading)
        {
            switch (weaponType)
            {
                case WeaponType.GravityGun:
                    // Gravity Gun: Sa� fare tu�una basarak kuvveti art�r, b�rak�ld���nda ate�le
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
                    // Makinal� T�fek: S�rekli ate�le
                    ShootRay(gunRange, bulletImpact);
                    break;

                case WeaponType.Pistol:
                    // Tabanca: Sol fare tu�una bas�ld���nda ate�le
                    if (Input.GetKeyDown(KeyCode.Mouse0)) ShootRay(gunRange, bulletImpact);
                    break;
            }
        }
    }

    void ShootRay(float fireDistance, GameObject bulletImpact)
    {
        // Ate� ediliyor mu ve yeniden y�kleniyor mu kontrol et
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
                    // Kur�unun yay�lma a��s�n� hesapla ve mermi etkisini olu�tur
                    float spreadX = Random.Range(-spreadAngle, spreadAngle);
                    float spreadY = Random.Range(0f, spreadAngle);

                    Vector3 spreadPosition = new(spreadX, spreadY, spreadX);
                    Instantiate(bulletImpact, hit.point + spreadPosition, Quaternion.identity);

                    // Silah�n rotasyonunu yay�lma a��s�na g�re ayarla
                    Quaternion spreadRotation = Quaternion.Euler(spreadX, spreadY, 0);
                    transform.rotation = spreadRotation;
                }
            }

            // Ate� etmeyi ba�lat ve bir s�re sonra durdur
            playerShoots = true;
            StartCoroutine(BasicTimer("playerShoots", firingRate));
        }
    }

    void Reload()
    {
        // Eksik mermileri hesapla ve yeniden y�kle
        int missingBullets = magazineBullet - _bullet;
        int bulletsToLoad = Mathf.Min(spareBullet, missingBullets);

        _bullet += bulletsToLoad;
        spareBullet -= bulletsToLoad;

        // Mermi say�lar�n� s�n�rla
        _bullet = Mathf.Clamp(_bullet, 0, magazineBullet);
        spareBullet = Mathf.Clamp(spareBullet, 0, 100);

        // Yeniden y�klemeyi ba�lat
        reloading = true;
        StartCoroutine(BasicTimer("reloading", 3f));
    }

    IEnumerator BasicTimer(string variableName, float timer)
    {
        // Belirtilen s�re boyunca bekle ve ilgili de�i�keni s�f�rla
        yield return new WaitForSeconds(timer);

        if (variableName == "playerShoots") { playerShoots = false; }
        if (variableName == "reloading") { reloading = false; }
    }
}
