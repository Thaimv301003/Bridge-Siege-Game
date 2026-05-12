using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class TutorialUnit : MonoBehaviour
    {
        // The tutorial element to enable when progressing through the tutorial
        [SerializeField] private GameObject tutorialToEnable;

        // References to essential game components that should be activated post-tutorial
        [SerializeField] private GameObject drawPad;
        [SerializeField] private GameObject spawner;

        // Banner ad to be displayed after completing the tutorial
        [SerializeField] private GameObject bannerAds;

        // Tracks whether the tutorial has been completed
        [SerializeField] private bool isDone;

        // ---------- Tutorial Button Logic ----------

        public void ButtonClicked()
        {
            if (isDone)
            {
                // Tutorial is completed
                gameObject.SetActive(false); // Hide the current tutorial unit
                PlayerPrefs.SetInt("TutorialDone", 1); // Mark the tutorial as completed

                bannerAds.SetActive(true); // Show banner ads

                drawPad.SetActive(true); // Activate drawing area
                spawner.SetActive(true); // Activate the spawner for gameplay
            }
            else
            {
                // Tutorial step is not completed yet
                gameObject.SetActive(false); // Hide the current tutorial unit
                tutorialToEnable.SetActive(true); // Enable the next tutorial part
            }
        }
    }
}
