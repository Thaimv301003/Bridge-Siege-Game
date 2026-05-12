using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class Bomb : MonoBehaviour
    {
        // Prefab for the explosion effect that occurs when the bomb detonates
        [SerializeField] private GameObject explodeEffect;

        // This method is called when the collider attached to this GameObject collides with another collider
        private void OnCollisionEnter(Collision other)
        {
            // Get the Vehicle component from the collided object (vehicle)
            Vehicle vehicle = other.collider.GetComponent<Vehicle>();
            
            // Check if the collided object is a Vehicle
            if (vehicle != null)
            {
                // Instantiate the explosion effect at the bomb's current position
                Instantiate(explodeEffect, transform.position, Quaternion.identity);
                
                // Deduct strength from the vehicle when it collides with the bomb
                vehicle.DeductStrength(2000f);
                
                // Destroy the bomb GameObject after the collision
                Destroy(gameObject);
            }
        }
    }
}
