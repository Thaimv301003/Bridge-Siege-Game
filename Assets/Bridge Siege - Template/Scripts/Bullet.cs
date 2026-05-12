using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class Bullet : MonoBehaviour
    {
        // Rigidbody component of the bullet for movement control
        public Rigidbody rb;

        // Speed at which the bullet travels
        public float speed = 10f;

        // Reference to explosion effect for the bullet
        public GameObject bulletExplode;

        // Amount of strength the bullet will deduct from a target (e.g., a vehicle)
        public float deductStrength = 11f;

        // Reference to the object pooler for efficient object reuse
        private ObjectPooler objectPooler;

        // Temporary booleans to ensure effects are applied only once
        private bool tempOnce_1 = true; // For speed enhancement
        private bool tempOnce_2 = true; // For damage enhancement

        // ---------- Initialization ----------

        private void Start()
        {
            // Get the instance of the object pooler for bullet and effect management
            objectPooler = ObjectPooler.instance;
        }

        // ---------- Bullet Activation Logic ----------

        private void OnEnable()
        {
            // Check if speed boost is active, and apply it only once
            if (GameManager.instance.isSpeedActivated && tempOnce_1)
            {
                speed *= 1.5f; // Increase bullet speed by 50%
                tempOnce_1 = false; // Prevents applying this multiple times
            }

            // Check if damage boost is active, and apply it only once
            if (GameManager.instance.isDamageActivated && tempOnce_2)
            {
                deductStrength *= 1.5f; // Increase bullet damage by 50%
                tempOnce_2 = false; // Prevents applying this multiple times
            }
        }

        // ---------- Bullet Movement ----------

        private void FixedUpdate()
        {
            // Sets the bullet's velocity, moving it forward based on its speed
            rb.linearVelocity = transform.up * speed;
        }

        // ---------- Collision Detection ----------

        private void OnTriggerEnter(Collider other)
        {
            // Spawn an explosion effect from the pool at the bullet's position
            objectPooler.SpawnFromPool("Bullet Effects", transform.position + new Vector3(0f, 0f, -2f), Quaternion.identity);

            // Check if the collided object has a Vehicle component
            Vehicle vehicle = other.GetComponent<Vehicle>();
            if (vehicle != null)
            {
                // Deducts strength from the vehicle upon impact
                vehicle.DeductStrength(deductStrength);
            }

            // Return the bullet to the pool for reuse
            objectPooler.BackToQueue("Bullets", gameObject);
        }
    }
}
