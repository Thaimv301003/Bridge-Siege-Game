using IndianOceanAssets.SlingMaster;
using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class Soldier : MonoBehaviour
    {
        // Initial time between each shot
        public float startTimeToShoot = .5f;
        
        // Time remaining until the next shot
        private float timeToShoot = 0f;

        // Position from where the soldier shoots
        public Transform ShootPos;

        // Prefab of the bullet and the shooting effect
        public GameObject bullet;
        public ParticleSystem fireEffect;

        // Ragdoll to instantiate upon soldier's death
        public GameObject ragdoll;

        // Layer mask to identify vehicles as targets
        public LayerMask vehicles;

        // Reference to DrawPad for managing soldier instances
        private DrawPad drawPad;

        // Object pooler instance for managing bullets
        private ObjectPooler objectPooler;

        // ---------- Initialization ----------

        private void Start()
        {
            // Set the shooting interval to the initial start time
            timeToShoot = startTimeToShoot;

            // Get the instance of the object pooler for spawning bullets
            objectPooler = ObjectPooler.instance;

            // Reference to the DrawPad for soldier management
            drawPad = FindObjectOfType<DrawPad>();

            // Play Spawn SFX
            AudioManager.Instance.Play( "SoldierSpawn" );
        }

        // ---------- Shooting Mechanism ----------

        private void Update()
        {
            // Check if it's time to shoot
            if (timeToShoot <= 0f)
            {
                ShootBullet(); // Fire a bullet
                timeToShoot = startTimeToShoot; // Reset the shooting timer
            }
            else
            {
                // Countdown to the next shot
                timeToShoot -= Time.deltaTime;
            }
        }

        // Method to shoot bullets at vehicles
        private void ShootBullet()
        {
            // Raycast to check for vehicles within shooting range
            if (Physics.Raycast(ShootPos.position, ShootPos.up, 40f, vehicles))
            {
                // Spawn a bullet from the object pool at the shooting position
                objectPooler.SpawnFromPool("Bullets", ShootPos.position, ShootPos.rotation);

                // Play the firing effect
                fireEffect.Play();
            }
        }

        // ---------- Collision Detection ----------

        // Detects collision with vehicles to trigger death
        private void OnCollisionEnter(Collision other)
        {
            if (other.collider.CompareTag("Vehicle"))
            {
                PlayerDied(); // Trigger death if the soldier collides with a vehicle
            }
        }

        // ---------- Death Mechanism ----------

        public void PlayerDied()
        {
            // Play Spawn SFX
            AudioManager.Instance.Play( "SoldierDie" );

            // Instantiate a ragdoll effect at the soldier's position
            Instantiate(ragdoll, transform.position, Quaternion.identity);

            // Remove the soldier from the DrawPad's soldier list
            drawPad.soldiers.Remove(gameObject);

            // Destroy the soldier object
            Destroy(gameObject);
        }
    }
}
