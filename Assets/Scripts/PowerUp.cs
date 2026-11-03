using UnityEngine;

[DisallowMultipleComponent]
public class PowerUp : MonoBehaviour
{
    public enum ResourceType
    {
        Health,
        Ammo
    }

    [SerializeField] private ResourceType resourceType = ResourceType.Health;
    [SerializeField, Min(1)] private int amount = 1;

    private bool collected;

    private void OnTriggerEnter(Collider other)
    {
        TryCollect(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // A full player can use this pickup later without having to leave the trigger first.
        TryCollect(other);
    }

    private void TryCollect(Collider other)
    {
        if (collected || amount <= 0)
            return;

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player == null || !player.isActiveAndEnabled || player.IsDead)
            return;

        switch (resourceType)
        {
            case ResourceType.Health:
                if (player.CurrentHealth >= player.MaxHealth)
                    return;

                // Guard before healing, which can notify other components through HealthChanged.
                collected = true;
                player.Heal(amount);
                break;

            case ResourceType.Ammo:
                PlayerShooting shooting = player.GetComponent<PlayerShooting>();
                if (shooting == null || !shooting.ReloadAmmo(amount))
                    return;

                collected = true;
                break;

            default:
                return;
        }

        // Disable immediately so multiple player colliders cannot collect it twice.
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
