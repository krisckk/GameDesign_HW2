
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MagicProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 5f;

    private Vector3 direction;
    private Transform owner;

    public void Initialize(Vector3 targetPosition, Transform caster)
    {
        owner = caster;
        direction = (targetPosition - transform.position).normalized;

        if (direction.sqrMagnitude < 0.001f)
            direction = transform.forward;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (owner != null &&
            (other.transform == owner ||
             other.transform.IsChildOf(owner)))
            return;

        if (other.CompareTag("Player"))
            Debug.Log("Magic hit Player!");

        Destroy(gameObject);
    }
}
