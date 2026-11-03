using UnityEngine;

[DisallowMultipleComponent]
public class EnemyContactDamage : MonoBehaviour
{
    [SerializeField, Min(1)] private int contactDamage = 1;

    private void OnCollisionEnter(Collision collision)
    {
        DamagePlayer(collision.collider);
    }

    private void OnCollisionStay(Collision collision)
    {
        // PlayerHealth decides whether its invulnerability has expired.
        DamagePlayer(collision.collider);
    }

    private void DamagePlayer(Collider other)
    {
        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage(contactDamage);
    }
}
