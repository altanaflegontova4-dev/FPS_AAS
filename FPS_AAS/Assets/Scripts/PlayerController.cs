using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    public float moveSpeed, gravityModifier, jumpPower, runSpeed;
    public CharacterController charCon;
    private Vector3 moveInput;
    public Transform camTrans;
    private int jumpAgain;
    public Animator anim;

    private Vector3 pushVelocity = Vector3.zero;
    public float pushDecay = 5f;

    public float mouseSensitivity;

    public GameObject bullet;

    public Gun activeGun;
    public List<Gun> allGuns = new List<Gun>();
    public int currentGun;

    AudioSource AS;
    AudioSource ASbg;
    AudioSource ASambient;

    public AudioClip[] walkStep;
    public AudioClip[] sprintStep;
    public AudioClip jumpclothSound;
    public AudioClip deathSound;
    public AudioClip switchgunSound;
    public AudioClip doorSound;
    public AudioClip reloadSound;
    public AudioClip usemedkitSound;
    public AudioClip useammoSound;
    public AudioClip outofammoSound;
    public AudioClip[] gethitSound;
    public AudioClip pickupSound;
    public AudioClip switchSound;
    public AudioClip healthfullSound;

    public AudioClip bgSound;
    public AudioClip ambientSound;

    public AudioClip noteSound;
    public AudioClip noteCloseSound;

    private int lastFootstep = -1;
    private float footstepTimer;
    public float walkStepDelay = 0.5f;
    public float sprintStepDelay = 0.25f;
    private float noAmmoCooldown = 0f;
    public float noAmmoDelay = 0.3f;

    [Header("Weapon Switch Delay")]
    [Tooltip("Время анимации убирания оружия перед доставанием нового")]
    public float switchGunDelay = 0.25f;

    [Header("Weapon Holder")]
    [Tooltip("Объект в камере, куда спавнится новое оружие (можно оставить пустым, тогда спавнится в camTrans)")]
    public Transform weaponHolder;
    private bool isSwitchingGun = false;

    public void Awake()
    {
        instance = this;
    }

    public void PlaySFX(AudioClip clip, float volume = 3f)
    {
        AS.PlayOneShot(clip, volume);
    }

    public void PlayRandomHitSound()
    {
        if (gethitSound.Length == 0) return;

        int index = Random.Range(0, gethitSound.Length);
        AS.PlayOneShot(gethitSound[index], 4.5f);
    }

    public void PlayRandomStepSound()
    {
        if (walkStep.Length == 0) return;

        int index = Random.Range(0, walkStep.Length);
        AS.PlayOneShot(walkStep[index]);
    }

    int randomIndex;

    void Start()
    {
        AudioSource[] sources = GetComponents<AudioSource>();

        AS = sources[0];
        ASbg = sources[1];
        ASambient = sources[2];

        ASbg.clip = bgSound;
        ASbg.loop = true;
        ASbg.volume = 0.15f;
        ASbg.Play();

        ASambient.clip = ambientSound;
        ASambient.loop = true;
        ASambient.volume = 0.1f;
        ASambient.Play();

        // Подготавливаем пули для всех имеющихся пушек
        for (int i = 0; i < allGuns.Count; i++)
        {
            if (allGuns[i] != null)
            {
                allGuns[i].PreparePool();
            }
        }

        // Активируем стартовое оружие
        if (allGuns.Count > 0 && currentGun < allGuns.Count && allGuns[currentGun] != null)
        {
            activeGun = allGuns[currentGun];
            activeGun.gameObject.SetActive(true);
            activeGun.PlayUnhide();
            activeGun.UpdateAmmoUI();
        }
        else
        {
            Debug.LogError("PlayerController: Список All Guns пуст или текущее оружие не назначено в Inspector!");
        }
    }

    void Update()
    {
        if (noAmmoCooldown > 0)
        {
            noAmmoCooldown -= Time.deltaTime;
        }

        // Логика отталкивания
        if (pushVelocity.magnitude > 0.1f)
        {
            charCon.Move(pushVelocity * Time.deltaTime);
            pushVelocity = Vector3.Lerp(pushVelocity, Vector3.zero, pushDecay * Time.deltaTime);
        }

        // Логика движения игрока
        float yStore = moveInput.y;

        Vector3 vertMove = transform.forward * Input.GetAxis("Vertical");
        Vector3 horiMove = transform.right * Input.GetAxis("Horizontal");

        moveInput = vertMove + horiMove;
        moveInput.Normalize();

        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveInput *= runSpeed;
        }
        else
        {
            moveInput *= moveSpeed;
        }

        moveInput.y = yStore;
        moveInput.y += Physics.gravity.y * gravityModifier * Time.deltaTime;

        if (charCon.isGrounded)
        {
            moveInput.y = -1f;
            moveInput.y += Physics.gravity.y * gravityModifier * Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                moveInput.y = jumpPower;
                jumpAgain = 2;

                AS.PlayOneShot(jumpclothSound, 2f);
            }
        }

        if (jumpAgain > 0 && Input.GetKeyDown(KeyCode.Space))
        {
            moveInput.y = jumpPower;
            jumpAgain--;
        }

        charCon.Move(moveInput * Time.deltaTime);

        float horizontalSpeed = new Vector3(charCon.velocity.x, 0, charCon.velocity.z).magnitude;
        if (anim != null)
        {
            anim.SetFloat("moveSpeed", horizontalSpeed);
        }

        // Footstep sounds
        if (charCon.isGrounded && horizontalSpeed > 0.1f)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                AudioClip[] currentSteps;
                float stepDelay;

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    currentSteps = sprintStep;
                    stepDelay = sprintStepDelay;
                }
                else
                {
                    currentSteps = walkStep;
                    stepDelay = walkStepDelay;
                }

                int randomIndex;

                do
                {
                    randomIndex = Random.Range(0, currentSteps.Length);
                }
                while (randomIndex == lastFootstep && currentSteps.Length > 1);

                lastFootstep = randomIndex;

                AS.PlayOneShot(currentSteps[randomIndex], 1.8f);

                footstepTimer = stepDelay;
            }
        }
        else
        {
            footstepTimer = 0f;
        }

        bool isMoving = charCon.velocity.magnitude > 0.1f;

       
        bool isMelee = activeGun != null && activeGun.isMelee;

       
        if (UIController.instance != null)
        {
            UIController.instance.SetCrosshairWalking(isMoving, isMelee);
        }

        // Поворот камеры и игрока
        Vector2 mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);
        camTrans.rotation = Quaternion.Euler(camTrans.rotation.eulerAngles + new Vector3(-mouseInput.y, 0f, 0f));

        // --- ЗАЩИТА: Если оружия нет, перезаряжается или меняется — выходим ---
        if (activeGun == null || activeGun.isReloading || isSwitchingGun) return;

        // Перезарядка
        if (Input.GetKeyDown(KeyCode.R))
        {
            AS.PlayOneShot(reloadSound, 2.5f);

            activeGun.Reload();
        }

        if (!activeGun.isMelee && Input.GetMouseButtonDown(0) &&activeGun.currentAmmo <= 0 && noAmmoCooldown <= 0f)
        {
            PlaySFX(outofammoSound, 3f);
            noAmmoCooldown = noAmmoDelay;
        }

        // Атака / Стрельба
        if (activeGun.isMelee || activeGun.currentAmmo > 0)
        {
            if (Input.GetMouseButtonDown(0) && activeGun.fireCounter <= 0)
            {
                fireShot();
            }
            if (Input.GetMouseButton(0) && activeGun.canAutoFire && activeGun.fireCounter <= 0)
            {
                fireShot();
            }
        }

        // Смена оружия
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            switchGun();
        }
    }

    public void ApplyPush(Vector3 direction, float force)
    {
        pushVelocity = direction * force;
    }

    public void fireShot()
    {
        if (activeGun == null) return;

        // 1. Анимация атаки
        activeGun.PlayFire();

        if (activeGun.isMelee)
        {
            if (activeGun.fireSound != null)
            {
                PlaySFX(activeGun.fireSound, 2f);
            }

            StartCoroutine(MeleeAttackDelayed());
        }

        // --- ЛОГИКА ОГНЕСТРЕЛА ---
        else
        {
            if (activeGun.currentAmmo <= 0)
            {
                if (noAmmoCooldown <= 0f)
                {
                    noAmmoCooldown = noAmmoDelay;
                }

                return;
            }

            if (activeGun.firePoint == null)
            {
                Debug.LogError("FirePoint не назначен на пушке: " + activeGun.gameObject.name);
                return;
            }

            RaycastHit hit;
            if (Physics.Raycast(camTrans.position, camTrans.forward, out hit, 50f))
            {
                activeGun.firePoint.LookAt(hit.point);
            }
            else
            {
                activeGun.firePoint.LookAt(camTrans.position + camTrans.forward * 30f);
            }

            activeGun.currentAmmo--;
           
            activeGun.GetBullet(activeGun.firePoint.position, activeGun.firePoint.rotation);

            if (activeGun.fireSound != null)
            {
                PlaySFX(activeGun.fireSound, 2f);
            }

            activeGun.PlayMuzzle();
        }

        activeGun.fireCounter = activeGun.fireRate;
        activeGun.UpdateAmmoUI();
    }

    public void switchGun()
    {
        AS.PlayOneShot(switchgunSound);

        if (allGuns.Count <= 1) return;
        StartCoroutine(SwitchGunCoroutine());
    }

    private IEnumerator SwitchGunCoroutine()
    {
        isSwitchingGun = true;

        // 1. Прячем старое оружие
        if (activeGun != null)
        {
            activeGun.PlayHide();
        }

        yield return new WaitForSeconds(switchGunDelay);

        // 2. Выключаем старое оружие
        if (activeGun != null)
        {
            activeGun.gameObject.SetActive(false);
        }

        // 3. Выбираем индекс нового оружия
        currentGun++;
        if (currentGun >= allGuns.Count)
        {
            currentGun = 0;
        }

        // 4. Присваиваем новое оружие в activeGun
        activeGun = allGuns[currentGun];

        // 5. Включаем новое оружие И ОБНОВЛЯЕМ ИКОНКУ НОВОГО ОРУЖИЯ
        if (activeGun != null)
        {
            activeGun.gameObject.SetActive(true);
            activeGun.PlayUnhide();
            activeGun.UpdateAmmoUI();

            // <-- ВОТ ЗДЕСЬ обновляем иконку для уже НОВОГО activeGun:
            if (UIController.instance != null)
            {
                UIController.instance.UpdateWeaponIcon(activeGun.gunIcon);
            }
        }

        isSwitchingGun = false;
    }

    public void AddWeapon(Gun gunPrefab)
    {
        if (gunPrefab == null) return;

        // 1. Проверяем: если такое оружие уже есть в инвентаре
        for (int i = 0; i < allGuns.Count; i++)
        {
            if (allGuns[i] != null && allGuns[i].ammoType == gunPrefab.ammoType)
            {
                // Если оружие уже есть — просто пополняем патроны
                allGuns[i].AddAmmo(allGuns[i].maxReserveAmmo);

                if (UIController.instance != null)
                {
                    UIController.instance.ShowMessage("Added ammo for " + allGuns[i].ammoType);
                }
                return;
            }
        }

        // 2. Если оружия нет — создаем его в руках у игрока
        Transform parentTransform = weaponHolder != null ? weaponHolder : camTrans;
        Gun newGun = Instantiate(gunPrefab, parentTransform);

        // Сохраняем локальную позицию и поворот префаба
        newGun.transform.localPosition = gunPrefab.transform.localPosition;
        newGun.transform.localRotation = gunPrefab.transform.localRotation;
        newGun.transform.localScale = gunPrefab.transform.localScale;

        // Подготавливаем пули для нового пулемета/автомата
        newGun.PreparePool();

        // 3. Выключаем текущее активное оружие
        if (activeGun != null)
        {
            activeGun.gameObject.SetActive(false);
        }

        // 4. Добавляем новое оружие в список и переключаемся на него
        allGuns.Add(newGun);
        currentGun = allGuns.Count - 1;
        activeGun = allGuns[currentGun];

        // Включаем и проигрываем анимацию доставания
        activeGun.gameObject.SetActive(true);
        activeGun.PlayUnhide();
        UIController.instance.UpdateWeaponIcon(gunPrefab.gunIcon);
        activeGun.UpdateAmmoUI();

        if (UIController.instance != null)
        {
            UIController.instance.ShowMessage("Picked up " + newGun.ammoType + "!");
        }
    }


    private IEnumerator MeleeAttackDelayed()
    {
        yield return new WaitForSeconds(activeGun.meleeDamageDelay);

        Debug.Log("Melee raycast firing");

        RaycastHit hit;
        Vector3 rayOrigin = camTrans.position + (camTrans.forward * 0.2f);

        if (Physics.Raycast(rayOrigin, camTrans.forward, out hit,
            activeGun.meleeRange,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Hit: " + hit.collider.gameObject.name);

            if (activeGun.hitEffectPrefab != null)
            {
                Debug.Log("Spawning effect at: " + hit.point);
                Vector3 newPosition = hit.point - camTrans.forward * 0.05f;
                ParticleSystem effect = Instantiate(
                    activeGun.hitEffectPrefab,
                    newPosition,
                    Quaternion.LookRotation(-camTrans.forward)
                );
                Destroy(effect.gameObject, 2f);
            }
            else
            {
                Debug.LogError("hitEffectPrefab is NULL!");
            }

            IDamagable damageable = hit.collider.GetComponentInParent<IDamagable>();
            if (damageable != null)
            {
                damageable.TakeDamage(activeGun.meleeDamage, false);
                Debug.Log("Damage dealt!");
            }
            else
            {
                Debug.Log("No IDamagable on: " + hit.collider.gameObject.name);
            }
        }
        else
        {
            Debug.Log("Raycast missed! Range: " + activeGun.meleeRange);
        }
    }
}