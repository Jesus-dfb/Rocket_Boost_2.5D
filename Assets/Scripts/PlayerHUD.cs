using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerShooting playerShooting;
    [SerializeField] private Text healthText;
    [SerializeField] private Text ammoText;

    private void OnEnable()
    {
        if (playerHealth == null)
            playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerShooting == null)
            playerShooting = FindAnyObjectByType<PlayerShooting>();

        if (playerHealth != null)
            playerHealth.HealthChanged += UpdateHealth;
        if (playerShooting != null)
            playerShooting.AmmoChanged += UpdateAmmo;
        Refresh();
    }

    private void Start()
    {
        // All player components have initialized their resources by Start.
        Refresh();
    }

    private void Refresh()
    {
        if (playerHealth != null)
            UpdateHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        if (playerShooting != null)
            UpdateAmmo(playerShooting.CurrentAmmo, playerShooting.MaxAmmo);
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged -= UpdateHealth;
        if (playerShooting != null)
            playerShooting.AmmoChanged -= UpdateAmmo;
    }

    private void UpdateHealth(int current, int maximum)
    {
        SetText(healthText, $"VIDA  {current} / {maximum}");
    }

    private void UpdateAmmo(int current, int maximum)
    {
        SetText(ammoText, playerShooting.InfiniteAmmo ? "MUNICIÓN  ∞" : $"MUNICIÓN  {current} / {maximum}");
    }

    private static void SetText(Text target, string value)
    {
        if (target == null) return;
        if (target.font == null)
            target.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        target.text = value;
    }
}
