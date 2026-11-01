
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MagicProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 10f;
    public float lifetime = 5f;

    [Header("Damage")]
    public int damage = 20;

    [Header("Effects")]
    public GameObject impactEffect;

    private Vector3 direction;
    private Transform owner;
    private bool hasHit = false;

    public void Initialize(
        Vector3 targetPosition,
        Transform caster)
    {
        owner = caster;

        direction =
            (targetPosition - transform.position).normalized;

        if (direction.sqrMagnitude < 0.001f)
            direction = transform.forward;

        transform.rotation =
            Quaternion.LookRotation(direction);
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (hasHit)
            return;

        transform.position +=
            direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit)
            return;

        if (owner != null &&
            (other.transform == owner ||
             other.transform.IsChildOf(owner)))
        {
            return;
        }

        PlayerHealth playerHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            hasHit = true;

            playerHealth.TakeDamage(damage);

            Debug.Log(
                "Mage Fireball Hit Player! Damage: " + damage
            );

            SpawnImpact();
            Destroy(gameObject);
            return;
        }


        if (other.GetComponentInParent<EnemyHealth>() != null)
            return;

        hasHit = true;

        SpawnImpact();
        Destroy(gameObject);
    }

    void SpawnImpact()
    {
        if (impactEffect != null)
        {
            Instantiate(
                impactEffect,
                transform.position,
                Quaternion.identity
            );
        }
    }
}
