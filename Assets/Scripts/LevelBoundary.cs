using UnityEngine;

public class LevelBoundary : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player != null) player.Kill();
    }
}
