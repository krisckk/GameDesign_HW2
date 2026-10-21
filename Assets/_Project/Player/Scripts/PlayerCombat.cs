using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    public Transform hitPoint;
    public float hitRadius = 1.2f;
    public int damage = 25;
    public float hitDelay = 0.25f;       // when the blade "lands" in the swing
    public float attackDuration = 0.6f;  // locked out of moving/attacking during this
    public LayerMask enemyLayers;
    public ParticleSystem slashFx;
    public AudioSource sfx;
    public AudioClip swingClip;

    InputSystem_Actions input;
    PlayerController controller;
    PlayerHealth health;

    void Awake()
    {
        input = new InputSystem_Actions();
        controller = GetComponent<PlayerController>();
        health = GetComponent<PlayerHealth>();
    }
    void OnEnable() => input.Player.Enable();
    void OnDisable() => input.Player.Disable();

    void Update()
    {
        if (!controller.canMove || !input.Player.Attack.WasPressedThisFrame()) return;
        controller.canMove = false;
        animator.SetTrigger("Attack");
        if (swingClip) sfx.PlayOneShot(swingClip);
        Invoke(nameof(DoHit), hitDelay);
        Invoke(nameof(EndAttack), attackDuration);
    }

    void DoHit()
    {
        if (slashFx) slashFx.Play();
        var hit = new HashSet<IDamageable>();
        foreach (var col in Physics.OverlapSphere(hitPoint.position, hitRadius, enemyLayers))
        {
            var target = col.GetComponentInParent<IDamageable>();
            if (target != null && hit.Add(target)) target.TakeDamage(damage);
        }
    }

    void EndAttack() { if (!health.IsDead) controller.canMove = true; }

    void OnDrawGizmosSelected() { if (hitPoint) Gizmos.DrawWireSphere(hitPoint.position, hitRadius); }
}