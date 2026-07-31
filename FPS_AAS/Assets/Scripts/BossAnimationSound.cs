using UnityEngine;

public class BossAnimationEvents : MonoBehaviour
{
    private BossController boss;

    void Awake()
    {
        boss = GetComponentInParent<BossController>();
    }

    public void PlayBossJumpSound()
    {
        if (boss != null)
            boss.PlayBossJumpSound();
    }

    public void PlayBossFootstep()
    {
        if (boss != null)
            boss.PlayBossFootstep();
    }

    public void PlayBossShootSound()
    {
        if (boss != null)
            boss.PlayBossShootSound();
    }

    public void PlayBossJumpAirSound()
    {
        if (boss != null)
            boss.PlayBossJumpAirSound();
    }

    public void PlayBossGetHitSound()
    {
        if (boss != null)
            boss.PlayBossGetHitSound();
    }

    public void PlayBossDieSound()
    {
        if (boss != null)
            boss.PlayBossDieSound();
    }
}