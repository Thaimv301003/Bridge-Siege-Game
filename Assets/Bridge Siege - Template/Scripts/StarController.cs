using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class StarController : MonoBehaviour
    {
        // Number of stars the player currently has
        [SerializeField] private int currentStars = 2;

        // Array of star GameObjects for the main game screen
        [SerializeField] private GameObject[] stars;

        // Array of star GameObjects for the win screen
        [SerializeField] private GameObject[] starsWinScreen;

        // Singleton instance for global access to StarController
        public static StarController instance;

        // Constructor to initialize the singleton instance
        StarController() => instance = this;

        // ---------- Initialization ----------

        private void Start()
        {
            // Activate all stars at the start of the game
            stars[0].SetActive(true);
            stars[1].SetActive(true);
            stars[2].SetActive(true);
        }

        // ---------- Star Reduction Mechanism ----------

        public void ReduceStars()
        {
            // Check if there are stars left to reduce
            if (currentStars > 0)
            {
                // Deactivate the star on both the main game screen and win screen
                stars[currentStars].SetActive(false);
                starsWinScreen[currentStars].SetActive(false);

                // Decrement the star count
                currentStars--;
            }
        }
    }
}
