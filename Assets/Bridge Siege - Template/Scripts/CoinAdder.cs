using IndianOceanAssets.SlingMaster;
using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class CoinAdder : MonoBehaviour
    {
        // Number of coins to add when this script is activated
        public int coinsToAdd = 100;

        // ---------- Initialization ----------

        private void Start()
        {
            // Adds the specified number of coins to the player's total using GameManager
            AudioManager.Instance.Play( "CoinAdded" );
            GameManager.instance.AddCoins(coinsToAdd);
        }
    }
}
