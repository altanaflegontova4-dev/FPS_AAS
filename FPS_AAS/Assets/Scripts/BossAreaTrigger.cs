using UnityEngine;

public class BossAreaTrigger : MonoBehaviour
{
    public BossController boss;

    public AudioSource bossMusic;
    public AudioClip bossMusicClip;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;

        boss.bossActivated = true;

        if (bossMusic != null && bossMusicClip != null)
        {
            bossMusic.clip = bossMusicClip;
            bossMusic.loop = true;
            bossMusic.Play();
        }
    }
}