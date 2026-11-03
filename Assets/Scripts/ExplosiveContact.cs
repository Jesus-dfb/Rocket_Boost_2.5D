using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyHealth))]
public class ExplosiveContact : MonoBehaviour
{
    private EnemyHealth health;
    private bool hasExploded;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryExplode(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryExplode(other);
    }

    private void TryExplode(Collider other)
    {
        if (hasExploded || health.IsDead)
            return;

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player == null || player.IsDead || !player.isActiveAndEnabled)
            return;

        hasExploded = true;
        player.Kill();
        // Reuse the explosion and destruction already handled by EnemyHealth.
        health.TakeDamage(health.CurrentHealth);
    }
}
