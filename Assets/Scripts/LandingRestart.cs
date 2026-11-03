using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LandingRestart : MonoBehaviour
{
    public float delay = 2.5f;
    private bool landed;
    private void OnCollisionEnter(Collision collision)
    {
        PlayerHealth player = collision.collider.GetComponentInParent<PlayerHealth>();
        if (landed || player == null || player.IsDead) return;
        landed = true;
        StartCoroutine(Restart());
    }
    private IEnumerator Restart()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
