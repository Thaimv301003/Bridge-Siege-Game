using System.Collections;
using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class TankShoot : MonoBehaviour
    {
        // Position from which the cannonball is fired
        public Transform shootPos;

        // Time interval between each shot
        public float timeToShoot = 3f;

        // Prefab of the cannonball to be instantiated
        public GameObject cannonBallPrefab;

        // ---------- Initialization ----------

        private void Start()
        {
            // Start the shooting coroutine with the specified interval
            StartCoroutine(Shoot());
        }

        // ---------- Shooting Mechanism ----------

        private IEnumerator Shoot()
        {
            // Wait for the specified shooting interval
            yield return new WaitForSeconds(timeToShoot);

            // Instantiate a cannonball at the shooting position and rotation
            Instantiate(cannonBallPrefab, shootPos.position, shootPos.rotation);

            // Restart the coroutine to continue shooting at regular intervals
            StartCoroutine(Shoot());
        }
    }
}
