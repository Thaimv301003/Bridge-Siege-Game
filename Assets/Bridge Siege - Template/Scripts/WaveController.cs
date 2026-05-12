using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class WaveController : MonoBehaviour
    {
        // ---------- Spawn Settings ----------
        
        [Header("Spawn Settings")]
        
        [Tooltip("Array of spawn points for vehicle spawning")]
        [SerializeField] private Transform[] spawnPoints; // Points where vehicles are spawned

        [Tooltip("Array of vehicle GameObjects to spawn at designated points")]
        [SerializeField] private GameObject[] vehicles; // Vehicle prefabs to be spawned

        // ---------- Wave Management ----------
        
        [Header("Wave Management")]
        private int currentWave = 0; // Tracks the current wave number

        [Tooltip("Number of waves the player has completed")]
        [Range(0, 100)]
        [SerializeField] private int noOfWaves; // Number of remaining waves

        private int totalWaves; // Total waves for the entire game

        [HideInInspector] public int isAllVehiclesDestroyed; // Counter to track remaining vehicles

        // ---------- Game Check Settings ----------
        
        [Header("Check Settings")]
        
        [Tooltip("Interval in seconds between checks for game conditions")]
        [Range(1f, 10f)]
        [SerializeField] private float timeBtwCheck = 3f; // Time delay between checks

        [Tooltip("Delay in seconds before spawning vehicles after incoming wave text appears (Player shop time)")]
        [Range(1f, 10f)]
        [SerializeField] private float timeBeforeSpawn = 3f; // Time before spawning vehicles

        // ---------- Animator Control ----------
        
        [Header("Animator Control")]
        
        [Tooltip("Animator component for controlling DrawPad animations")]
        [SerializeField] private Animator drawPadAnimator; // Controls drawPad animations

        // ---------- Lane Settings ----------
        
        [Header("Lane Settings")]
        
        [Tooltip("Total number of lanes available for vehicle spawning")]
        [Range(1, 10)]
        [SerializeField] private int noOfLanes = 3; // Total lanes for spawning

        [Tooltip("List indicating the availability of each lane (true if available, false if occupied)")]
        [SerializeField] private List<bool> lanes; // Tracks availability of each lane

        // ---------- Initialization ----------

        private void Start()
        {
            totalWaves = noOfWaves; // Set total waves initially

            GameManager.instance.WaveCounterUI(currentWave, noOfWaves); // Update UI with initial wave count

            GameManager.instance.SetShopInteractable(true); // Allow shop at the beginning

            StartCoroutine(Manager()); // Start wave management coroutine
        }

        // ---------- Wave and Vehicle Management ----------

        // Decrement vehicle count and check if all waves are cleared
        public void DecrementVehiclesCount()
        {
            isAllVehiclesDestroyed--; // Reduce vehicle count when one is destroyed

            if (noOfWaves == 0 && isAllVehiclesDestroyed == 0) // Check if all waves are complete
            {
                StartCoroutine(UI()); // Trigger UI update for mission completion
            }
        }

        // Display mission completion UI after a delay
        IEnumerator UI()
        {
            yield return new WaitForSeconds(1.5f);
            GameManager.instance.missionAccomplished(); // Call mission accomplished in GameManager
        }

        // Spawns vehicles at random lanes for each wave
        void Spawn()
        {
            int selectRandomLanes = Random.Range(0, 3); // Select a random number of lanes for spawning

            lanes = new List<bool>(); // Reset lane availability

            // Set all lanes as available initially
            for (int i = 0; i < noOfLanes; i++)
            {
                lanes.Add(true);
            }

            // Randomly mark some lanes as occupied based on the selected number
            for (int i = 0; i < noOfLanes; i++)
            {
                if (selectRandomLanes > 0)
                {
                    lanes[Random.Range(0, noOfLanes)] = false;
                    selectRandomLanes--;
                }
            }

            // Spawn vehicles in available lanes
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (lanes[i]) // Check if lane is available
                {
                    Instantiate(vehicles[Random.Range(0, vehicles.Length)], spawnPoints[i].position, spawnPoints[i].rotation);
                    isAllVehiclesDestroyed++; // Increase vehicle count
                }
            }

            noOfWaves--; // Decrement remaining waves count
        }

        // ---------- Wave Management Coroutine ----------

        // Coroutine to manage waves and spawn conditions
        IEnumerator Manager()
        {
            yield return new WaitForSeconds(timeBtwCheck); // Wait between checks

            // Check if there are waves left and all vehicles are destroyed
            if (noOfWaves > 0 && isAllVehiclesDestroyed == 0)
            {
                GameManager.instance.SetShopInteractable(true); // Enable shop during transition

                currentWave++; // Increment wave count

                GameManager.instance.WaveCounterUI(currentWave, totalWaves); // Update UI with wave count

                GameManager.instance.EnableIncomingWave(); // Show incoming wave indicator

                // Trigger tutorial animation if it's the player's first game
                if (PlayerPrefs.GetInt("TutorialDone", 0) == 0)
                {
                    if (drawPadAnimator != null)
                    {
                        drawPadAnimator.SetTrigger("DRAW");
                    }
                    else
                    {
                        Debug.LogWarning("drawPadAnimator is not assigned in WaveController. Please assign it in the Inspector.");
                    }
                }

                yield return new WaitForSeconds(timeBeforeSpawn); // Additional delay before spawning

                GameManager.instance.SetShopInteractable(false); // Disable shop when wave starts
                Spawn(); // Spawn the next wave of vehicles
            }

            StartCoroutine(Manager()); // Restart the coroutine for continuous checking
        }

        // Spawns a bomb at a random position within the spawn points
        public void SpawnBomb(GameObject bomb)
        {
            // Offset the bomb's position for variety
            Instantiate(bomb, spawnPoints[Random.Range(0, spawnPoints.Length)].position + new Vector3(0f, 3f, -Random.Range(20f, 40f)), Quaternion.identity);
        }
    }
}
