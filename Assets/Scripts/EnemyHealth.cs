using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField, Min(1)] private int maxHealth = 3;
    [SerializeField] private int currentHealth;

    [Header("Death Effect")]
    [SerializeField] private ParticleSystem explosionPrefab;

    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);
    }

    public bool TakeDamage(int damage)
    {
        if (!isActiveAndEnabled || IsDead || damage <= 0)
            return false;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        if (IsDead)
            Die();

        return true;
    }

    private void Die()
    {
        // Assign and configure the particle prefab in the Unity Inspector.
        if (explosionPrefab != null)
        {
            ParticleSystem explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            explosion.Play();
        }

        // Disable contacts immediately; Destroy removes the object at the end of the frame.
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
