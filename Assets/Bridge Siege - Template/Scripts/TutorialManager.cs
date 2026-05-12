using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class TutorialManager : MonoBehaviour
    {
        // Reference to the hand animation GameObject used in the tutorial
        [SerializeField] private GameObject handAnimator;

        // ---------- Initialization ----------

        private void Start()
        {
            // Check if the tutorial has been completed by reading the "TutorialDone" key
            // If the tutorial hasn't been completed, enable the hand animation
            if (PlayerPrefs.GetInt("TutorialDone", 0) == 0)
            {
                handAnimator.SetActive(true);
            }
        }
    }
}
