using System.Collections;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healAmount = 25;
    public float healDuration = 0.4f;   // 血量在這段時間內逐步補滿，血條看得出在回升
    public AudioClip pickupClip;
    public Material effectMaterial;     // URP ParticlesUnlit

    static readonly Color HealGreen = new Color(0.35f, 1f, 0.45f);

    bool used;

    void OnTriggerEnter(Collider other)
    {
        TryPickup(other);
    }

    // 滿血時站在道具上不會撿走，之後受傷仍可撿起
    void OnTriggerStay(Collider other)
    {
        TryPickup(other);
    }

    void TryPickup(Collider other)
    {
        if (used) return;

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player == null || player.IsDead) return;
        if (PlayerStats.Instance.currentHP >= PlayerStats.Instance.maxHP) return;

        used = true;

        // 協程掛在玩家身上，道具被刪除後仍會繼續補血
        player.StartCoroutine(HealOverTime(player, healAmount, healDuration));

        if (effectMaterial != null)
        {
            SpawnPickupBurst(transform.position);
            SpawnHealAura(player.transform);
        }

        if (pickupClip != null)
            AudioSource.PlayClipAtPoint(pickupClip, transform.position);

        Destroy(gameObject);
    }

    static IEnumerator HealOverTime(PlayerHealth player, int amount, float duration)
    {
        int healed = 0;
        float elapsed = 0f;
        duration = Mathf.Max(duration, 0.01f);

        while (healed < amount)
        {
            elapsed += Time.deltaTime;

            int target = Mathf.Min(amount, Mathf.CeilToInt(amount * elapsed / duration));

            if (target > healed)
            {
                player.Heal(target - healed);
                healed = target;
            }

            yield return null;
        }
    }

    // 道具位置：綠白色光點向外炸開
    void SpawnPickupBurst(Vector3 position)
    {
        ParticleSystem ps = CreateParticleSystem("HealthPickupBurst", null);
        ps.transform.position = position;

        ParticleSystem.MainModule main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.18f);
        main.startColor = new ParticleSystem.MinMaxGradient(HealGreen, Color.white);
        main.gravityModifier = 0.3f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        ps.Play();
    }

    // 玩家身上：光點從腳邊往上飄
    void SpawnHealAura(Transform player)
    {
        ParticleSystem ps = CreateParticleSystem("HealAura", player);
        ps.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        ps.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        ParticleSystem.MainModule main = ps.main;
        main.duration = 0.8f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 1.6f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.14f);
        main.startColor = new ParticleSystem.MinMaxGradient(HealGreen, Color.white);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 50f;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 5f;
        shape.radius = 0.4f;

        ps.Play();
    }

    ParticleSystem CreateParticleSystem(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);   // 新增時會自動播放，先停下才能修改 duration

        ParticleSystem.MainModule main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.stopAction = ParticleSystemStopAction.Destroy;

        ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        ps.GetComponent<ParticleSystemRenderer>().material = effectMaterial;

        return ps;
    }
}
