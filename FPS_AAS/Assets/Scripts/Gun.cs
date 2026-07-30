using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Bullet Pool")]
    public BulletController bulletPrefab;
    public int poolSize = 30;

    [Header("Gun Settings")]
    public Transform firePoint;
    public bool canAutoFire;
    public float fireRate;
    [HideInInspector]
    public float fireCounter;

    [Header("Melee Settings")]
    public bool isMelee;
    public int meleeDamage;
    public float meleeRange = 2f;
    public float meleeDamageDelay = 0.3f; 


    [Header("Ammo")]
    public string ammoType;
    public int currentAmmo;       // ammo in magazine right now
    public int magazineSize;      // magazine size
    public int reserveAmmo;       // ammo in reserve
    public int maxReserveAmmo;    // max ammo can be in reserve
    public float reloadTime = 2.0f;
    public bool isReloading = false;

    [Header("UI")]
    public Sprite gunIcon;

    [Header("Animation")]
    public Animator weaponAnim;
   
    public ParticleSystem muzzleEffect;

    public ParticleSystem hitEffectPrefab;

    private Queue<BulletController> bulletPool = new Queue<BulletController>();
    private Transform poolParent;
    private bool poolReady;

    void Update()
    {
        if (fireCounter > 0)
        {
            fireCounter -= Time.deltaTime;
        }
    }

    private void OnDisable()
    {
        // Если оружие убрали во время перезарядки — сбрасываем статус
        isReloading = false;
        StopAllCoroutines();
    }

    public void Reload()
    {
        if (isMelee) return;

        if (!isReloading && currentAmmo < magazineSize && reserveAmmo > 0)
        {
            StartCoroutine(ReloadCoroutine());
        }
        if (reserveAmmo == 0 && UIController.instance != null)
        {
            UIController.instance.ammoText.text = "Out of ammo";
        }
    }

    public void PlayFire()
    {
        if (weaponAnim != null)
            weaponAnim.SetTrigger("Fire");
    }

    public void PlayReload()
    {
        if (weaponAnim != null)
            weaponAnim.SetTrigger("Reload");
    }

    public void PlayHide()
    {
        if (weaponAnim != null)
            weaponAnim.SetTrigger("Hide");
    }

    public void PlayUnhide()
    {
        if (weaponAnim != null)
            weaponAnim.SetTrigger("Unhide");
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;

        // Запускаем анимацию перезарядки
        PlayReload();

        if (UIController.instance != null)
        {
            UIController.instance.ammoText.text = "RELOADING...";
        }

        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = magazineSize - currentAmmo;

        if (reserveAmmo >= ammoNeeded)
        {
            currentAmmo = magazineSize;
            reserveAmmo -= ammoNeeded;
        }
        else
        {
            currentAmmo += reserveAmmo;
            reserveAmmo = 0;
        }

        isReloading = false;
        UpdateAmmoUI();
    }

    public void UpdateAmmoUI()
    {
        if (UIController.instance == null) return;

        if (isMelee)
        {
            UIController.instance.ammoText.text = "∞";
        }
        else
        {
            UIController.instance.ammoText.text = "" + currentAmmo + " / " + reserveAmmo;
        }
    }

    public void AddAmmo(int amount)
    {
        if (isMelee) return;

        reserveAmmo += amount;
        if (reserveAmmo > maxReserveAmmo)
        {
            reserveAmmo = maxReserveAmmo;
        }
        UpdateAmmoUI();
    }

    public void PlayMuzzle()
    {
        if (muzzleEffect != null)
        {
            muzzleEffect.Clear(); // очищаем старые частицы
            muzzleEffect.Play();
        }
    }


    public void PreparePool()
    {
        // ЗАЩИТА: Ножу или оружию без префаба пуль пул не нужен!
        if (isMelee || bulletPrefab == null) return;
        if (poolReady) return;

        GameObject parentObj = new GameObject(gameObject.name + "_BulletPool");
        poolParent = parentObj.transform;

        for (int i = 0; i < poolSize; i++)
        {
            BulletController bullet = Instantiate(bulletPrefab, poolParent);
            bullet.gameObject.SetActive(false);
            bullet.SetReturnAction(ReturnBullet);
            bulletPool.Enqueue(bullet);
        }

        poolReady = true;
    }

    public BulletController GetBullet(Vector3 position, Quaternion rotation)
    {
        if (isMelee) return null;

        PreparePool();

        BulletController bullet;

        if (bulletPool.Count > 0)
        {
            bullet = bulletPool.Dequeue();
        }
        else
        {
            bullet = Instantiate(bulletPrefab, poolParent);
            bullet.SetReturnAction(ReturnBullet);
        }

        bullet.transform.SetPositionAndRotation(position, rotation);
        bullet.gameObject.SetActive(true);
        bullet.Fire();

        return bullet;
    }

    public void ReturnBullet(BulletController bullet)
    {
        if (bullet == null) return;

        bullet.gameObject.SetActive(false);
        bulletPool.Enqueue(bullet);
    }
}