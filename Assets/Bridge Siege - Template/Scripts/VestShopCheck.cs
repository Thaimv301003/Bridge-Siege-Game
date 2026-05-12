using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class VestShopCheck : MonoBehaviour
    {
        // References to the bat and shield GameObjects
        public GameObject bat;
        public GameObject shield;

        // ---------- Initialization ----------

        private void Start()
        {
            // Check if the shield is activated in the GameManager
            if (GameManager.instance.isShieldActivated)
            {
                // Enable the shield if it's been activated
                shield.SetActive(true);
            }
        }
    }
}
