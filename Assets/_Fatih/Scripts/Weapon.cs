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
    [SerializeField] GameObject hitEffect; // Kur�un �arpt���nda kullan�lacak efekt
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
    [SerializeField] float fireDistance; // Ate� etme mesafesi

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
        // Maksimum mermi say�s�n� anl�k mermi say�s�yla e�itle
        magazineBullet = _bullet;
        startRot = transform.localRotation;
        weaponManager = GetComponentInParent<WeaponManager>();
    }

    private void Update()
    {
        // Oyuncu ate� edebilir mi kontrol et ve ate�le
        if (playerCanShoot)
        {
            SwitchWeapon();
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

    private void LateUpdate()
    {
        if (Input.GetKey(KeyCode.Mouse0) && playerShoots && !reloading)
        {
            if (_bullet > 0)
            {
                {
                    // Silah�n rotasyonunu yay�lma a��s�na g�re ayarla
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
        // Ate� etmiyorsa ve yeniden y�klenmiyorsa ate�le
        if (!playerShoots && !reloading)
        {
            switch (weaponType)
            {
                case WeaponType.GravityGun:
                    // Gravity Gun: Sa� fare tu�una basarak kuvveti art�r, b�rak�ld���nda ate�le
                    // if (Input.GetKeyDown(KeyCode.Mouse0)) Fire();
                    GravityGun();
                    break;

                case WeaponType.Rifle:
                    // Makinal� T�fek: S�rekli ate�le
                    Fire();
                    if (Input.GetMouseButtonDown(1)) GetComponent<RifleScope>().Scope();
                    break;

                case WeaponType.Pistol:
                    // Tabanca: Sol fare tu�una bas�ld���nda ate�le
                    if (Input.GetMouseButtonDown(0)) Fire();
                    break;

                case WeaponType.Knife:
                    // B��ak
                    KnifeAttacks();
                    break;
            }
        }
    }

    void Fire()
    {
        // Ate� ediliyor mu ve yeniden y�kleniyor mu kontrol et
        if (Input.GetMouseButton(0) && !playerShoots && !reloading)
        {
            if (_bullet > 0)
            {
                // Mermiyi azalt
                _bullet--;
                muzzleVFX.Play();

                ShootRay();
            }

            // Ate� etmeyi ba�lat ve bir s�re sonra durdur
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

            // Oyuncunun bak�� y�n�n� al
            Transform playerTransform = playerCont.gameObject.transform;
            Vector3 lookDirection = playerTransform.forward;

            // Kuvvet y�n�n� belirle ve tersine �evir
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
        // Eksik mermileri hesapla ve yeniden y�kle
        int missingBullets = magazineBullet - _bullet;
        int bulletsToLoad = Mathf.Min(spareBullet, missingBullets);

        _bullet += bulletsToLoad;
        spareBullet -= bulletsToLoad;

        // Mermi say�lar�n� s�n�rla
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
        // Belirtilen s�re boyunca bekle ve ilgili de�i�keni s�f�rla
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
