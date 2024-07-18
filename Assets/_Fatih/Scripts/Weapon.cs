using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class Weapon : MonoBehaviour
{
    // Silah t�rlerini belirten bir enum
    public enum WeaponType { Knife, Pistol, Rifle, GravityGun };
    public WeaponType weaponType;

    Quaternion startRot;

    [SerializeField] LayerMask interactionLayer; // Etkile�im katman�
    [SerializeField] GameObject bulletImpact; // Kur�un �arpt���nda kullan�lacak efekt
    [SerializeField] Camera cam; // Silah�n kullanaca�� kamera

    [Header("General Specs")]
    [SerializeField] int Damage; // Silah�n verdi�i hasar
    [SerializeField] int spareBullet; // Yedek mermi say�s�
    [SerializeField] int magazineBullet; // Silah�n �arj�r� doluyken alabilece�i maksimum mermi say�s�
    [SerializeField] int _bullet; // Anl�k mermi say�s�
    [SerializeField] int currentRecoilIndex;
    [SerializeField] bool reloading; // Yeniden y�kleme yap�l�yor mu?
    [SerializeField] bool playerShoots; // Oyuncu �u anda ate� ediyor mu?
    [SerializeField] bool playerCanShoot = true; // Oyuncu ate� edebilir mi?
    [SerializeField] float firingRate; // Ate� etme h�z�
    [SerializeField] float gunRange; // Ate� etme mesafesi

    [Header("Gravity Gun")]
    [SerializeField] float gravityGunForce; // Gravity Gun kuvveti
    [SerializeField] VisualEffect muzzleVFX;

    WeaponManager weaponManager;
    Animator anim;

    Vector3 hitPoint;
    private void Start()
    {
        // Maksimum mermi say�s�n� anl�k mermi say�s�yla e�itle
        magazineBullet = _bullet;
        startRot = transform.localRotation;
        weaponManager = GetComponentInParent<WeaponManager>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // Oyuncu ate� edebilir mi kontrol et ve ate�le
        if (playerCanShoot)
        {
            Fire();
        }

        // R tu�una bas�ld���nda ve mermiler bitti�inde yeniden y�kle
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
        // Ate� etmiyorsa ve yeniden y�klenmiyorsa ate�le
        if (!playerShoots && !reloading)
        {
            switch (weaponType)
            {
                case WeaponType.GravityGun:
                    // Gravity Gun: Sa� fare tu�una basarak kuvveti art�r, b�rak�ld���nda ate�le
                    GravityGun();
                    break;

                case WeaponType.Rifle:
                    // Makinal� T�fek: S�rekli ate�le
                    ShootRay(gunRange, bulletImpact);
                    if (Input.GetKeyDown(KeyCode.Mouse1)) GetComponent<RifleScope>().Scope();
                    break;

                case WeaponType.Pistol:
                    // Tabanca: Sol fare tu�una bas�ld���nda ate�le
                    if (Input.GetKeyDown(KeyCode.Mouse0)) ShootRay(gunRange, bulletImpact);
                    break;

                case WeaponType.Knife:
                    // B��ak
                    if (Input.GetKeyDown(KeyCode.Mouse0)) Debug.Log("sol atak");
                    if (Input.GetKeyDown(KeyCode.Mouse1)) Debug.Log("sa� atak");
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
                // Ate� etmeyi ba�lat ve bir s�re sonra durdur
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

                // Silah�n rotasyonunu yay�lma a��s�na g�re ayarla
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

            // Oyuncunun bak�� y�n�n� al
            Transform playerTransform = playerCont.gameObject.transform;
            Vector3 lookDirection = playerTransform.forward;

            // Kuvvet y�n�n� belirle ve tersine �evir
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
        // Eksik mermileri hesapla ve yeniden y�kle
        int missingBullets = magazineBullet - _bullet;
        int bulletsToLoad = Mathf.Min(spareBullet, missingBullets);

        _bullet += bulletsToLoad;
        spareBullet -= bulletsToLoad;

        // Mermi say�lar�n� s�n�rla
        _bullet = Mathf.Clamp(_bullet, 0, magazineBullet);
        spareBullet = Mathf.Clamp(spareBullet, 0, 100);

        reloading = false;
    }

    IEnumerator BasicTimer(string variableName, float timer)
    {
        // Belirtilen s�re boyunca bekle ve ilgili de�i�keni s�f�rla
        yield return new WaitForSeconds(timer);

        if (variableName == "playerShoots") { playerShoots = false; } 
    }
}
