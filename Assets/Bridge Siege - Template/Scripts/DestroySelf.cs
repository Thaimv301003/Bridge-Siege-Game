using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class DestroySelf : MonoBehaviour
    {
        // Time in seconds before the object destroys itself
        public float destroyTimer = 2f;

        // ---------- Initialization ----------

        private void Start()
        {
            // Schedules this object to be destroyed after the specified time
            Destroy(gameObject, destroyTimer);
        }
    }
}
