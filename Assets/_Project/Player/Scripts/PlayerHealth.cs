using System;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public static event Action Died;     // GameManager listens -> show Game Over

    public Animator animator;
    public AudioSource sfx;
    public AudioClip hurtClip, deathClip;
    public ParticleSystem hurtFx;
    public float invincibleTime = 0.5f;
    public CinemachineInputAxisController cameraInput;   // drag PlayerFreeLookCam here

    public bool IsDead { get; private set; }
    float lastHitTime = -99f;

    public void TakeDamage(int amount)
    {
        if (IsDead || Time.time < lastHitTime + invincibleTime) return;
        lastHitTime = Time.time;
        PlayerStats.Instance.ChangeHP(-amount);
        if (hurtFx) hurtFx.Play();

        if (PlayerStats.Instance.currentHP <= 0)
        {
            IsDead = true;
            if (cameraInput) cameraInput.enabled = false;   // stop mouse from spinning the camera
            GetComponent<PlayerController>().canMove = false;
            animator.SetTrigger("Die");
            if (deathClip) sfx.PlayOneShot(deathClip);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Died?.Invoke();
        }
        else
        {
            animator.SetTrigger("Hit");
            if (hurtClip) sfx.PlayOneShot(hurtClip);
        }
    }

    public void Heal(int amount) { if (!IsDead) PlayerStats.Instance.ChangeHP(amount); }
}