using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class VehicleDestroyer : MonoBehaviour
    {
        // ---------- Collision Detection ----------

        private void OnTriggerEnter(Collider other)
        {
            // Check if the colliding object has the "Vehicle" tag
            if (other.CompareTag("Vehicle"))
            {
                // Destroy the vehicle upon collision
                Destroy(other.gameObject);
            }
        }
    }
}
