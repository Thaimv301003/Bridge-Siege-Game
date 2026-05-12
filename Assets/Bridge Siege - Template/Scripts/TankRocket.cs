using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class TankRocket : MonoBehaviour
    {
        // Rigidbody component for controlling the rocket's movement
        public Rigidbody rb;

        // Speed at which the rocket travels
        public float speed = 40f;

        // Explosion effect prefab to be instantiated on impact
        public GameObject explodeEffect;

        // Layer mask to detect soldiers affected by the rocket's explosion
        public LayerMask whatIsSoldier;

        // ---------- Rocket Movement ----------

        private void FixedUpdate()
        {
            // Set the rocket's velocity to move forward based on its speed
            rb.linearVelocity = transform.forward * speed;
        }

        // ---------- Collision Detection and Explosion ----------

        private void OnTriggerEnter(Collider other)
        {
            // Instantiate explosion effect at the rocket's position
            Instantiate(explodeEffect, transform.position, Quaternion.identity);

            // Detect all soldiers within the explosion radius
            Collider[] cols = Physics.OverlapSphere(transform.position, 2f, whatIsSoldier);

            // Loop through each detected soldier and trigger their death method
            foreach (Collider C in cols)
            {
                C.GetComponent<Soldier>().PlayerDied();
            }

            // Destroy the rocket after the explosion
            Destroy(gameObject);
        }
    }
}
