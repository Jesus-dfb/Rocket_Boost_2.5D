using UnityEngine;

public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField, Min(0.01f)] private float bulletLifeTime = 1f;

    [Header("Impact Effect")]
    [SerializeField] private ParticleSystem impactPrefab;

    private int damage = 1;
    private Transform owner;
    private bool hasHit;

    public void Initialize(int bulletDamage, Transform bulletOwner)
    {
        damage = Mathf.Max(1, bulletDamage);
        owner = bulletOwner;
    }

    void OnEnable()
    {
        hasHit = false;
        foreach (ParticleSystem effect in GetComponentsInChildren<ParticleSystem>())
        {
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            effect.Play(true);
        }
        CancelInvoke(nameof(DeactivateBullet));
        Invoke(nameof(DeactivateBullet), bulletLifeTime);
    }

    void OnDisable()
    {
        // A previous shot must not deactivate this bullet when the pool reuses it.
        CancelInvoke(nameof(DeactivateBullet));
        foreach (ParticleSystem effect in GetComponentsInChildren<ParticleSystem>())
            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    /// <summary>
    /// Deactivates the bullet
    /// </summary>
    private void DeactivateBullet()
    {
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (hasHit || !isActiveAndEnabled)
            return;

        // Ignore the shooter and other bullets.
        if (owner != null && (other.transform == owner || other.transform.IsChildOf(owner)))
            return;
        if (other.GetComponentInParent<BulletController>() != null)
            return;

        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        // Future pickups and other trigger zones should not stop a shot.
        if (other.isTrigger && enemyHealth == null)
            return;

        hasHit = true;
        PlayImpact(other);
        if (enemyHealth != null)
            enemyHealth.TakeDamage(damage);

        DeactivateBullet();
    }

    private void PlayImpact(Collider other)
    {
        if (impactPrefab == null)
            return;

        Vector3 velocity = GetComponent<Rigidbody>().linearVelocity;
        Vector3 incomingDirection = velocity.sqrMagnitude > 0.001f ? velocity.normalized : transform.up;
        // Triggers have no contact normal: approximate it from the surface nearest the previous position.
        Vector3 previousPosition = transform.position - velocity * Time.fixedDeltaTime;
        Vector3 impactPosition = other.ClosestPoint(previousPosition);
        Vector3 normal = previousPosition - impactPosition;
        if (normal.sqrMagnitude < 0.001f)
            normal = -incomingDirection;

        ParticleSystem impact = Instantiate(impactPrefab, impactPosition, Quaternion.LookRotation(normal));
        impact.Play();
    }


}
