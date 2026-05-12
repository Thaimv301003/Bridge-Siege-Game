using UnityEngine;
using System.Collections;

namespace IndianOceanAssets.BridgeSiege
{
    public class EnemyShoot : MonoBehaviour
    {
        // Array of shooting positions for the enemy
        [SerializeField] private Transform[] shootPos;

        // Layer mask to detect player objects
        [SerializeField] private LayerMask whatIsPlayer;

        // Time interval between shots, adjustable between 0.5 and 3 seconds
        [Range(.5f, 3f)]
        [SerializeField] private float timeToShoot = 1.25f;

        // Index to track the current shooting position
        private int index = 0;

        // ---------- Initialization ----------

        void Start()
        {
            // Start the shooting coroutine with a delay specified by timeToShoot
            StartCoroutine(Shoot(timeToShoot));
        }

        // ---------- Shooting Logic ----------

        // Coroutine to handle the shooting process
        IEnumerator Shoot(float time)
        {
            // Stop if the index exceeds the array length
            if (index >= (shootPos.Length - 1))
                yield break;

            // Wait for the specified shooting interval
            yield return new WaitForSeconds(time);

            // Cast a ray to detect the player from the current shooting position
            Physics.Raycast(shootPos[index].position, transform.forward, out RaycastHit hit, 100f, whatIsPlayer);

            // If the ray hits the player, trigger the player's death and move to the next position
            if (hit.collider != null)
            {
                hit.collider.GetComponent<Soldier>().PlayerDied(); // Call player death method
                index++; // Move to the next shooting position
                StartCoroutine(Shoot(timeToShoot)); // Restart shooting with the regular interval
            }
            else
            {
                // If the player isn't hit, attempt another shot immediately
                StartCoroutine(Shoot(0f));
            }
        }
    }
}
