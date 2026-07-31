using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossHealthController : MonoBehaviour, IDamagable
{
    [Header("Health & Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public Animator anim;

    [Header("Death Effects")]
    public ParticleSystem smokeEffect;
    public ParticleSystem electricEffect;
    public Transform[] effectSpawnPoints;

    public BossController bossController;

    private bool isDead;
    public float hitStunDuration = 0.5f;
    public float destroyDelay = 6f;

    void Start()
    {
        currentHealth = maxHealth;

        // скрываем health bar босса до начала боя
        if (UIController.instance.bossHealthPanel != null)
            UIController.instance.bossHealthPanel.SetActive(false);
    }

   
    public void ActivateBossUI()
    {
        if (UIController.instance.bossHealthPanel != null)
            UIController.instance.bossHealthPanel.SetActive(true);

        if (UIController.instance.bossHealthSlider != null)
        {
            UIController.instance.bossHealthSlider.maxValue = maxHealth;
            UIController.instance.bossHealthSlider.value = currentHealth;
        }

        if (UIController.instance.bossHealthText != null)
            UIController.instance.bossHealthText.text = "OVERSEER: " + currentHealth + "/" + maxHealth;
    }

    void UpdateBossUI()
    {
        if (UIController.instance.bossHealthSlider != null)
            UIController.instance.bossHealthSlider.value = currentHealth;

        if (UIController.instance.bossHealthText != null)
            UIController.instance.bossHealthText.text = "OVERSEER: " + currentHealth + "/" + maxHealth;
    }

    public void TakeDamage(int damage, bool attackPlayer)
    {
        if (attackPlayer || isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateBossUI();

        // проверяем rage режим
        if (currentHealth <= maxHealth / 2 && bossController != null)
        {
            bossController.EnterRageMode();
        }

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (anim != null)
        {
            anim.SetBool("IsMoving", false);
            anim.SetTrigger("Hit");
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        if (bossController != null)
            bossController.StunByHit(hitStunDuration);
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Die");

        PlayDeathEffects();

        if (UIController.instance.bossHealthPanel != null)
            UIController.instance.bossHealthPanel.SetActive(false);

        if (bossController != null)
        {
            bossController.CancelAttacks();
            bossController.enabled = false;
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        // запускаем финальную катсцену через задержку
        StartCoroutine(OutroDelay());
    }

    IEnumerator OutroDelay()
    {
        // ждём пока анимация смерти босса доиграет
        yield return new WaitForSeconds(destroyDelay - 1f);

        if (CutsceneManager.instance != null)
            CutsceneManager.instance.PlayOutro();
    }

    void PlayDeathEffects()
    {
        // если есть точки спавна — играем в каждой точке
        if (effectSpawnPoints != null && effectSpawnPoints.Length > 0)
        {
            foreach (Transform point in effectSpawnPoints)
            {
                if (smokeEffect != null)
                {
                    ParticleSystem smoke = Instantiate(smokeEffect, point.position, point.rotation);
                    Destroy(smoke.gameObject, destroyDelay);
                }

                if (electricEffect != null)
                {
                    ParticleSystem electric = Instantiate(electricEffect, point.position, point.rotation);
                    Destroy(electric.gameObject, destroyDelay);
                }
            }
        }
        else
        {
            // если точек нет — играем на позиции босса
            if (smokeEffect != null)
            {
                ParticleSystem smoke = Instantiate(smokeEffect, transform.position, transform.rotation);
                Destroy(smoke.gameObject, destroyDelay);
            }

            if (electricEffect != null)
            {
                ParticleSystem electric = Instantiate(electricEffect, transform.position, transform.rotation);
                Destroy(electric.gameObject, destroyDelay);
            }
        }
    }
}