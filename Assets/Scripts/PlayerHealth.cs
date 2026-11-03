using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField, Min(1)] private int maxHealth = 3;
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 1f;
    [SerializeField] private int currentHealth;

    [Header("Damage Blink")]
    [SerializeField, Min(0.01f)] private float blinkInterval = 0.1f;

    private Renderer[] playerRenderers;
    private bool[] rendererVisibility;
    private bool isBlinking;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;
    public bool IsInvulnerable { get; private set; }

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;
        // Mesh renderers only: keep the thrust and explosion particles visible.
        playerRenderers = Array.FindAll(GetComponentsInChildren<Renderer>(true),
            renderer => renderer is MeshRenderer || renderer is SkinnedMeshRenderer);
        rendererVisibility = new bool[playerRenderers.Length];
    }

    /// <summary>Returns true only when damage is accepted.</summary>
    public bool TakeDamage(int damage = 1)
    {
        if (!isActiveAndEnabled || IsDead || IsInvulnerable || damage <= 0)
            return false;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        // Set protection before notifying listeners to prevent repeated damage.
        if (!IsDead)
            StartCoroutine(Invulnerability());

        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (IsDead)
            Died?.Invoke();

        return true;
    }

    /// <summary>Instant death, including during temporary damage protection.</summary>
    public void Kill()
    {
        if (!isActiveAndEnabled || IsDead)
            return;

        currentHealth = 0;
        StopAllCoroutines();
        RestoreVisibility();
        IsInvulnerable = false;
        HealthChanged?.Invoke(currentHealth, maxHealth);
        Died?.Invoke();
    }

    /// <summary>Restores health without exceeding the maximum or reviving the player.</summary>
    public void Heal(int amount)
    {
        if (!isActiveAndEnabled || IsDead || amount <= 0)
            return;

        int restoredHealth = currentHealth + Mathf.Min(amount, maxHealth - currentHealth);
        if (restoredHealth == currentHealth)
            return;

        currentHealth = restoredHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private IEnumerator Invulnerability()
    {
        IsInvulnerable = true;
        isBlinking = true;
        for (int i = 0; i < playerRenderers.Length; i++)
            rendererVisibility[i] = playerRenderers[i] != null && playerRenderers[i].enabled;

        float startTime = Time.time;
        while (Time.time - startTime < invulnerabilityDuration)
        {
            float elapsed = Time.time - startTime;
            bool visible = Mathf.FloorToInt(elapsed / Mathf.Max(0.01f, blinkInterval)) % 2 != 0;
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                if (playerRenderers[i] != null)
                    playerRenderers[i].enabled = visible && rendererVisibility[i];
            }
            yield return null;
        }

        RestoreVisibility();
        IsInvulnerable = false;
    }

    private void RestoreVisibility()
    {
        if (!isBlinking)
            return;

        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
                playerRenderers[i].enabled = rendererVisibility[i];
        }
        isBlinking = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        RestoreVisibility();
        IsInvulnerable = false;
    }
}
