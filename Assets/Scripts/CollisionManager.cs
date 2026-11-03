using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerHealth))]
public class CollisionManager : MonoBehaviour
{

    [Header("Audio Settings")]
    [SerializeField] private AudioClip explosionAudioClip;
    [SerializeField] private AudioClip landingAudioClip;

    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem explosionParticleSystem;
    [SerializeField] private ParticleSystem landingParticleSystem;

    [Header("Death Settings")]
    [SerializeField, Min(0f)] private float restartDelay = 2f;

    AudioSource audioSource;
    PlayerHealth playerHealth;

    bool isControllable = true;
    bool isRestarting;


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void OnEnable()
    {
        playerHealth.Died += HandleDeath;
    }

    void OnDisable()
    {
        playerHealth.Died -= HandleDeath;
    }

    private void HandleDeath()
    {
        if (isRestarting) return;
        isRestarting = true;

        if (isControllable)
            ManageCollision(explosionAudioClip, explosionParticleSystem);

        StartCoroutine(RestartLevel());
    }

    private IEnumerator RestartLevel()
    {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!isControllable) return;

        switch (collision.gameObject.tag)
        {
            case "LaunchPad":
                Debug.Log("LaunchPad Collision Detected");
                break;
            case "LandingPad":
                ManageCollision(landingAudioClip, landingParticleSystem);
                break;
            case "Explosion":
                playerHealth.TakeDamage(1);
                break;
            default:                
                break;
        }
    }

    /// <summary>
    /// Manages the collision event by disabling player control, 
    /// stopping any ongoing audio, playing the specified audio clip, 
    /// playing the specified particle system, 
    /// and disabling the PlayerMovement script.
    /// </summary>
    /// <param name="audioClip"></param>
    private void ManageCollision(AudioClip audioClip, ParticleSystem particleSystem)
    {
        isControllable = false;
        audioSource.Stop();
        audioSource.PlayOneShot(audioClip);
        particleSystem.Play();
        GetComponent<PlayerMovement>().enabled = false;        
    }
    

    
}
