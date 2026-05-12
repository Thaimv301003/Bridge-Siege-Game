using UnityEngine;
using System.Collections;

namespace IndianOceanAssets.BridgeSiege
{
    public class TutorialEnabler : MonoBehaviour
    {
        // ---------- Initialization ----------

        void Start()
        {
            // Start the coroutine to disable the tutorial after a delay
            StartCoroutine(DisableInfinity());
        }

        // ---------- Coroutine to Disable Tutorial ----------

        IEnumerator DisableInfinity()
        {
            // Wait for 4 seconds before deactivating the game object
            yield return new WaitForSeconds(4f);

            // Deactivate this game object, effectively ending the tutorial
            gameObject.SetActive(false);
        }
    }
}
