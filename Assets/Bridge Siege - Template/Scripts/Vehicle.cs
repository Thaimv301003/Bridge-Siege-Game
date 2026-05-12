using IndianOceanAssets.SlingMaster;
using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class Vehicle : MonoBehaviour
    {
        // Rigidbody component attached to the vehicle
        public Rigidbody rb;

        // Speed settings for the vehicle
        [Header("Speed Settings")]
        private float speed = 10f;
        public float minSpeed = 8f; // Minimum speed of the vehicle
        public float maxSpeed = 20f; // Maximum speed of the vehicle

        // Strength of the vehicle, which determines how much damage it can take
        [Header("Vehicle Properties")]
        public float strength = 100f; // Initial strength of the vehicle

        // Prefabs for explosions and coin spawning
        [Header("Prefabs")]
        public GameObject Explosive; // Prefab for the explosive effect
        public GameObject coinToSpawn; // Prefab for spawning coins

        // Array for body parts of the vehicle
        [Space]
        public GameObject[] bodyParts;

        // Reference to the wave controller
        private WaveController waveController;

        // Ragdoll prefab for goons
        [SerializeField] private GameObject ragdoll; // Ragdoll prefab for goons
        [SerializeField] private GameObject[] goons; // Array of goon GameObjects

        private void Start()
        {
            // Set a random speed within the specified range
            speed = Random.Range(minSpeed, maxSpeed);
            // Find and assign the WaveController in the scene
            waveController = FindObjectOfType<WaveController>();
        }

        void FixedUpdate()
        {
            // Move the vehicle downward based on speed
            rb.linearVelocity = new Vector3(0f, -1f, transform.forward.z * speed);
        }

        // Method to deduct strength from the vehicle
        public void DeductStrength(float amount)
        {
            strength -= amount; // Reduce strength by the specified amount
            
            // Check if the strength is less than or equal to zero
            if (strength <= 0f)
            {
                AudioManager.Instance.Play("VehicleExplode");
                
                // Iterate through body parts and apply explosion force
                foreach (GameObject G in bodyParts)
                {
                    // Check if the body part doesn't have a Rigidbody
                    if (G.GetComponent<Rigidbody>() == null)
                    {
                        // Add a Rigidbody and apply explosion force
                        Rigidbody r = G.AddComponent<Rigidbody>();
                        r.AddExplosionForce(10f, transform.position, 10f, 1f, ForceMode.Impulse);
                    }
                }

                // Destroy the goons associated with this vehicle
                DestroyGoons();

                // Spawn a coin at the vehicle's position
                Instantiate(coinToSpawn, transform.position, Quaternion.identity);

                // Instantiate the explosive effect
                Instantiate(Explosive, transform.position, Quaternion.identity);
                
                // Disable the collider for this vehicle
                GetComponent<BoxCollider>().enabled = false;
                
                // Add a self-destruction component with a timer
                gameObject.AddComponent<DestroySelf>().destroyTimer = 3f;

                // Decrement the vehicle count in the wave controller
                waveController.DecrementVehiclesCount();

                // Check if all vehicles are destroyed
                if (waveController.isAllVehiclesDestroyed == 0)
                {
                    GameManager.instance.drawPad.DestroySoldiers();
                }

                // Destroy this vehicle instance
                Destroy(this);
            }
        }

        // Method to destroy goons and spawn ragdolls
        public void DestroyGoons()
        {
            foreach (GameObject G in goons)
            {
                
                // Instantiate ragdoll at the goon's position and rotation
                Instantiate(ragdoll, G.transform.position, G.transform.rotation);
                // Destroy the goon GameObject
                Destroy(G);
            }
        }
    }
}
