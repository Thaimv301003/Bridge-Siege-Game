using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class CopsCar : MonoBehaviour
    {
        // Prefab to instantiate when the car explodes
        [SerializeField] private GameObject Explode;

        // This method is called when the collider attached to this GameObject collides with another collider
        private void OnCollisionEnter(Collision other)
        {
            // Check if the collided object has the tag "Vehicle" and has a Vehicle component
            if (other.collider.CompareTag("Vehicle") && other.collider.GetComponent<Vehicle>() != null)
            {
                // Instantiate the explosion effect at the point of contact
                Instantiate(Explode, other.contacts[0].point, Quaternion.identity);

                // Apply an explosion force to the Rigidbody of this CopsCar
                GetComponent<Rigidbody>().AddExplosionForce(12f, other.contacts[0].point, 4f, .2f, ForceMode.Impulse);
                
                // Call the method to destroy soldiers in the DrawPad
                FindObjectOfType<DrawPad>().DestroySoldiers();

                // Invoke the UI method after a delay of 2 seconds
                Invoke("UI", 2f);
            }
        }

        // Method to handle the UI changes when the mission fails
        void UI()
        {
            // Call the missionFailed method on the GameManager instance
            GameManager.instance.missionFailed();
        }
    }
}
