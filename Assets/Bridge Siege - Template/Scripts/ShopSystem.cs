using IndianOceanAssets.SlingMaster;
using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class ShopSystem : MonoBehaviour
    {
        // ---------- Shop UI Elements ----------

        [Header("Shop UI Elements")]
        
        [Tooltip("The main shop window GameObject")]
        public GameObject shopWindow; // Main UI for the shop

        [Tooltip("Screen displayed when the player has insufficient funds")]
        [SerializeField] private GameObject lessMoneyScreen; // UI shown when player lacks funds

        // ---------- Shop Items ----------

        [Header("Shop Items")]
        
        [Tooltip("Array of amounts corresponding to each item available for purchase")]
        [SerializeField] private int[] amount; // Cost of each shop item

        [Tooltip("Array of buttons to buy items in the shop")]
        [SerializeField] private GameObject[] buyButton; // Buttons for purchasing items

        [Tooltip("Array of buttons to equip items from the shop")]
        [SerializeField] private GameObject[] equipButton; // Buttons to equip purchased items

        [Tooltip("Array of buttons indicating which items are currently equipped")]
        [SerializeField] private GameObject[] equippedButton; // Buttons indicating equipped items

        // ---------- Player Currency Status ----------

        [Header("Current Status")]
        
        [Tooltip("Current amount of currency the player has")]
        [SerializeField] private int currentAmount = 0; // Player's available currency

        // ---------- Initialization ----------

        private void Start()
        {
            // Load the player's currency from PlayerPrefs
            currentAmount = PlayerPrefs.GetInt("Coins", 0);
        }

        // ---------- Shop Window Controls ----------

        // Close the shop window and resume gameplay
        public void CloseShopWindow()
        {
            AudioManager.Instance.Play( "ButtonClick" );
            GameManager.instance.drawPadArea.SetActive(true); // Reactivate drawing area
            GameManager.instance.pauseButton.SetActive(true); // Show pause button
            Time.timeScale = 1f; // Resume game time
            shopWindow.SetActive(false); // Hide shop window
        }

        // Open the shop window and pause gameplay
        public void OpenShopWindow()
        {
            AudioManager.Instance.Play( "ButtonClick" );
            GameManager.instance.drawPadArea.SetActive(false); // Deactivate drawing area
            GameManager.instance.pauseButton.SetActive(false); // Hide pause button
            Time.timeScale = 0f; // Pause game time
            shopWindow.SetActive(true); // Show shop window
        }

        // ---------- Item Purchase and Equip ----------

        public void EquipSkin(int index)
        {
            // Refresh current currency amount
            currentAmount = PlayerPrefs.GetInt("Coins", 0);

            // Check if player has enough currency for the item
            if (amount[index] <= currentAmount)
            {
                buyButton[index].SetActive(false); // Hide the buy button for this item
                equippedButton[index].SetActive(true); // Show the equipped indicator

                // Apply the purchased item’s effect based on its index
                switch (index)
                {
                    case 0:
                        GameManager.instance.isDamageActivated = true; // Activate damage boost
                        break;
                    case 1:
                        GameManager.instance.SpawnBombWaveController(); // Spawn a bomb wave controller
                        break;
                    case 2:
                        GameManager.instance.isSpeedActivated = true; // Activate speed boost
                        break;
                    case 3:
                        GameManager.instance.isShieldActivated = true; // Activate shield boost
                        break;
                    default:
                        break;
                }

                // Deduct the item's cost from the player's currency
                currentAmount -= amount[index];
                PlayerPrefs.SetInt("Coins", currentAmount); // Save updated currency

                // Update the displayed coin count
                GameManager.instance.AddCoins(0);
                Debug.Log("Purchase Successful for Item Index: " + index);
            }
            else
            {
                // Show the "Insufficient Funds" screen if player can't afford item
                lessMoneyScreen.SetActive(true);
            }
        }

        // Hide the "Insufficient Funds" screen
        public void DisableLessMoneyScreen()
        {
            lessMoneyScreen.SetActive(false);
        }
    }
}
