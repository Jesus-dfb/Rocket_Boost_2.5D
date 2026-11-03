using System;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Ammo Settings")]
    [SerializeField, Min(0)] private int currentAmmo = 10;
    [SerializeField, Min(1)] private int maxAmmo = 10;
    [SerializeField] private bool infiniteAmmo = false;

    [Header("Shooting Settings")]
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletRate = 0.25f;
    [SerializeField] private int bulletDamage = 1;

    private Transform bulletSpawnPoint;
    private ObjectPool bulletPool;

    private float lastShotTime = 0f;

    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo;
    public bool InfiniteAmmo => infiniteAmmo;
    public event Action<int, int> AmmoChanged;

    private void Awake()
    {
        maxAmmo = Mathf.Max(1, maxAmmo);
        currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);
    }

    /// <summary>Returns true when ammo was restored. Infinite ammo does not consume pickups.</summary>
    public bool ReloadAmmo(int amount)
    {
        if (!isActiveAndEnabled || infiniteAmmo || amount <= 0 || currentAmmo >= maxAmmo)
            return false;

        currentAmmo += Mathf.Min(amount, maxAmmo - currentAmmo);
        AmmoChanged?.Invoke(currentAmmo, maxAmmo);
        return true;
    }

    void Start()
    {
        bulletSpawnPoint = transform.Find("ShootPoint");
        bulletPool = GetComponent<ObjectPool>();
    }

    /// <summary>
    /// Check if the player can shoot based on the bullet rate and ammo availability
    /// </summary>
    /// <returns></returns>
    public bool CanShoot()
    {        
        //Check if enough time has passed since the last shot and if there is ammo available 
        // or infinite ammo is enabled
        return (Time.time - lastShotTime >= bulletRate) && (infiniteAmmo || currentAmmo > 0);
     
    }

    /// <summary>
    /// Shoot a bullet from the object pool and apply 
    /// </summary>
    public void Shoot()
    {
        if (CanShoot())
        {
            //update last shot time
            lastShotTime = Time.time;    

            //reduce the Ammo
            if (!infiniteAmmo)
            {
                currentAmmo--;
                AmmoChanged?.Invoke(currentAmmo, maxAmmo);
            }

            //Get a bullet from the object pool
            // Prepare the shot before activation, which starts the lifetime timer.
            GameObject bullet = bulletPool.GetObjectFromPool(false);
            bullet.transform.position = bulletSpawnPoint.position;
            bullet.transform.rotation = bulletSpawnPoint.rotation;
            bullet.GetComponent<BulletController>().Initialize(bulletDamage, transform);
            bullet.SetActive(true);

            //Set the bullet's speed and movement forward
            Rigidbody bulletRigidbody = bullet.GetComponent<Rigidbody>();
            bulletRigidbody.linearVelocity = bulletSpawnPoint.up * bulletSpeed;
         
        }
     
    }
}
