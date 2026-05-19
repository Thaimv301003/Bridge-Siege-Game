using IndianOceanAssets.SlingMaster;
using UnityEngine;
using TheLegends.Base.Ads; // Added for Rewarded Ads

namespace IndianOceanAssets.BridgeSiege
{
    public class ShopSystem : MonoBehaviour
    {
        // ---------- Shop UI Elements ----------

        [Header("Shop UI Elements")]
        
        [Tooltip("The main shop window GameObject")]
        public GameObject shopWindow; // Main UI for the shop

        // ---------- Shop Items ----------

        [Header("Shop Items")]
        
        [Tooltip("Array of amounts corresponding to each item available for purchase")]
        [SerializeField] private int[] amount; // Cost of each shop item

        [Tooltip("Array of buttons to buy items in the shop")]
        [SerializeField] private GameObject[] buyButton; // Buttons for purchasing items

        [Tooltip("Array of buttons indicating which items are currently equipped")]
        [SerializeField] private GameObject[] equippedButton; // Buttons indicating equipped items

        [Tooltip("Array of buttons shown when the player doesn't have enough money")]
        [SerializeField] private GameObject[] notEnoughMoneyButton; // Indicators for insufficient funds

        // ---------- Player Currency Status ----------

        [Header("Current Status")]
        
        [Tooltip("Current amount of currency the player has")]
        [SerializeField] private int currentAmount = 0; // Player's available currency

        // ---------- Initialization ----------

        public bool isOpen { get; private set; } = false;

        private void Start()
        {
            // Load the player's currency from PlayerPrefs
            currentAmount = PlayerPrefs.GetInt("Coins", 0);
        }

        // ---------- Shop Window Controls ----------

        // Close the shop window and resume gameplay
        public void CloseShopWindow()
        {
            isOpen = false;
            if (AudioManager.Instance != null) AudioManager.Instance.Play( "ButtonClick" );
            if (GameManager.instance != null && GameManager.instance.drawPadArea != null) GameManager.instance.drawPadArea.SetActive(true); // Reactivate drawing area
            if (GameManager.instance != null && GameManager.instance.pauseButton != null) GameManager.instance.pauseButton.SetActive(true); // Show pause button
            Time.timeScale = 1f; // Resume game time
            if (shopWindow != null) shopWindow.SetActive(false); // Hide shop window
        }

        // Open the shop window and pause gameplay
        public void OpenShopWindow()
        {
            isOpen = true;
            if (AudioManager.Instance != null) AudioManager.Instance.Play( "ButtonClick" );
            if (GameManager.instance != null && GameManager.instance.drawPadArea != null) GameManager.instance.drawPadArea.SetActive(false); // Deactivate drawing area
            // Keep pause button visible when shop is open
            // if (GameManager.instance != null && GameManager.instance.pauseButton != null) GameManager.instance.pauseButton.SetActive(false); // Hide pause button
            Time.timeScale = 0f; // Pause game time
            if (shopWindow != null) shopWindow.SetActive(true); // Show shop window
            
            UpdateShopUI(); // Automatically update button states when opening
        }

        // ---------- Item Purchase and Equip ----------

        // Automatically update the visibility of Buy vs. NotEnoughMoney buttons
        private void UpdateShopUI()
        {
            currentAmount = PlayerPrefs.GetInt("Coins", 0);

            for (int i = 0; i < amount.Length; i++)
            {
                // If already equipped, keep other buttons hidden
                if (equippedButton[i] != null && equippedButton[i].activeSelf)
                {
                    if (buyButton[i] != null) buyButton[i].SetActive(false);
                    if (notEnoughMoneyButton[i] != null) notEnoughMoneyButton[i].SetActive(false);
                    continue;
                }

                // Item 0 (Ads) is always available
                if (i == 0)
                {
                    if (buyButton[i] != null) buyButton[i].SetActive(true);
                    if (notEnoughMoneyButton[i] != null) notEnoughMoneyButton[i].SetActive(false);
                    continue;
                }

                // For other items, check if player can afford it
                if (currentAmount >= amount[i])
                {
                    if (buyButton[i] != null) buyButton[i].SetActive(true);
                    if (notEnoughMoneyButton[i] != null) notEnoughMoneyButton[i].SetActive(false);
                }
                else
                {
                    if (buyButton[i] != null) buyButton[i].SetActive(false);
                    if (notEnoughMoneyButton[i] != null) notEnoughMoneyButton[i].SetActive(true);
                }
            }
        }

        // Helper method to activate the item's effect and update UI
        private void ApplyItemEffect(int index)
        {
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

            // Update UI buttons
            if (index == 1)
            {
                // For bombs (consumable), do not show equipped button. Keep buy button available.
                if (index < equippedButton.Length && equippedButton[index] != null)
                    equippedButton[index].SetActive(false);
            }
            else
            {
                if (index < buyButton.Length && buyButton[index] != null)
                    buyButton[index].SetActive(false); // Hide the buy button
                
                if (index < notEnoughMoneyButton.Length && notEnoughMoneyButton[index] != null)
                    notEnoughMoneyButton[index].SetActive(false); // Hide the not enough money button
                
                if (index < equippedButton.Length && equippedButton[index] != null)
                    equippedButton[index].SetActive(true); // Show the equipped indicator
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.Play("ButtonClick");
            
            UpdateShopUI(); // Refresh UI for other items after a purchase
        }

        // Method to buy item with coins (Assigned to the coin text/button)
        public void EquipSkin(int index)
        {
            // If it's item 0 (High Damage), force watching an ad instead of using coins
            if (index == 0)
            {
                BuyWithAds(index);
                return;
            }

            // Refresh current currency amount
            currentAmount = PlayerPrefs.GetInt("Coins", 0);

            // Check if player has enough currency for the item
            if (amount[index] <= currentAmount)
            {
                // Deduct the item's cost from the player's currency
                currentAmount -= amount[index];
                PlayerPrefs.SetInt("Coins", currentAmount); // Save updated currency

                // Update the displayed coin count
                GameManager.instance.AddCoins(0);

                // Activate the item
                ApplyItemEffect(index);
                
                Debug.Log("Purchase Successful with Coins for Item Index: " + index);
            }
        }

        // Method to buy item by watching a Rewarded Ad (Assigned to the item image button)
        public void BuyWithAds(int index)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play("ButtonClick");

#if UNITY_EDITOR
            Debug.Log("Bypassing Ads in Unity Editor for Shop...");
            ApplyItemEffect(index);
            StartCoroutine(KeepTimePaused());
            return;
#endif

            // Show Rewarded Ad
            TheLegends.Base.Ads.AdsManager.Instance.ShowRewarded(TheLegends.Base.Ads.PlacementOrder.One, "shop_free_item_" + index, () => {
                // Success callback: Give item for free
                ApplyItemEffect(index);
                Debug.Log("Purchase Successful with Ads for Item Index: " + index);
                
                // Gọi Coroutine để chắc chắn game vẫn dừng sau khi Ads SDK khôi phục timeScale
                StartCoroutine(KeepTimePaused());
            });
        }

        private System.Collections.IEnumerator KeepTimePaused()
        {
            yield return new WaitForSecondsRealtime(0.1f); // Đợi một chút để Ads SDK xử lý xong
            if (shopWindow != null && shopWindow.activeInHierarchy)
            {
                Time.timeScale = 0f;
            }
        }

    }
}
