using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using IndianOceanAssets.SlingMaster;
using TheLegends.Base.Firebase;
using TheLegends.Base.Ads;
using TheLegends.Base.Ads;

namespace IndianOceanAssets.BridgeSiege
{
    public class GameManager : MonoBehaviour
    {
        [Header("Platform Settings")]
        [Tooltip("Define the platform the game is running on, either MOBILE or PC")]
        public GamePlatform gamePlatform;

        public enum GamePlatform
        {
            MOBILE, // Mobile platform
            PC // PC platform
        }

        [Space(10)]
        [Header("Level Management")]
        [Tooltip("Total number of levels in the game")]
        [SerializeField] private int totalNoOfLevels;

        [Tooltip("Tracks the current level number")]
        private int levelNo = 1;

        [Tooltip("Singleton instance of GameManager")]
        public static GameManager instance;

        [Space(10)]
        [Header("Draw Pad Settings")]
        [Tooltip("Reference to DrawPad script for handling drawing mechanics")]
        public DrawPad drawPad;

        [Tooltip("UI area for drawing interactions")]
        public GameObject drawPadArea;

        [Space(10)]
        [Header("Wave and Level UI")]
        [Tooltip("Notification for incoming wave")]
        [SerializeField] private GameObject incomingWave;

        [Tooltip("UI for advancing to the next level")]
        [SerializeField] private GameObject nextLevel;

        [Tooltip("Button for advancing to the next level")]
        [SerializeField] private GameObject nextLevelButton;

        [Tooltip("UI for retrying the level")]
        [SerializeField] private GameObject retryLevel;

        [Tooltip("Button for retrying the level")]
        [SerializeField] private GameObject retryLevelButton;

        [Tooltip("Controls wave functionality")]
        [SerializeField] private GameObject waveController;

        [Tooltip("Displays money or currency UI")]
        [SerializeField] private GameObject moneyUI;

        [Space(10)]
        [Header("Shop System")]
        [Tooltip("Button to access the in-game shop")]
        [SerializeField] private GameObject shopButton;

        [Tooltip("Reference to ShopSystem script")]
        public ShopSystem shopSystem;

        [Space(10)]
        [Header("Pause Menu")]
        [Tooltip("UI for pause window")]
        public GameObject pauseWindow;

        [Tooltip("Button to trigger the pause menu")]
        public GameObject pauseButton;

        [Space(10)]
        [Header("Coins and Rewards")]
        [Tooltip("Text displaying coins earned in the current level")]
        [SerializeField] private TMPro.TMP_Text coinEarnedThisLevelText;

        [Tooltip("Text hiển thị tổng số tiền người chơi đang có trên bảng Victory")]
        public TMPro.TMP_Text totalMoneyVictoryText;

        [Tooltip("Text hiển thị số tiền nhận được ở màn chơi hiện tại trên bảng Victory")]
        public TMPro.TMP_Text baseCoinVictoryText;

        [Tooltip("Counter for coins earned in the current level")]
        private int coinEarnedThisLevel = 0;

        [Tooltip("Reward screen UI shown after level completion")]
        [SerializeField] private GameObject rewardScreen;

        [Tooltip("Object pháo hoa xuất hiện khi chiến thắng màn chơi")]
        [SerializeField] private GameObject fireworksObject;


        [Space(10)]
        [Header("Level and Currency Display")]
        [Tooltip("UI text for displaying the current level")]
        [SerializeField] private TMPro.TMP_Text levelText;

        [Tooltip("UI text for displaying total coins")]
        [SerializeField] private TMPro.TMP_Text coinText;

        [Space(10)]
        [Header("Touch Controls")]
        [Tooltip("Slider UI for touch controls (for mobile platform)")]
        public Image touchSlider;

        [Tooltip("GameObject for touch slider UI container")]
        public GameObject touchSliderObj;

        [Space(10)]
        [Header("Reward System")]
        [Tooltip("Checks if the current scene has rewards")]
        [SerializeField] private bool isRewardedScene = false;

        [Tooltip("Unique reward key identifier")]
        public string rewardKey;

        [Tooltip("UI text displaying wave information")]
        [SerializeField] private TMPro.TMP_Text waveUI;

        [Tooltip("Reference to RewardSystem script for handling rewards")]
        public RewardSystem rewardSystem;

        [Space(10)]
        [Header("Gun Customization")]
        [Tooltip("Material for applying gun skins")]
        [SerializeField] private Material gunSkin;

        [Space(10)]
        [Header("Shop Enhancements")]
        [Tooltip("Flag to indicate if shield enhancement is active")]
        public bool isShieldActivated = false;

        [Tooltip("Flag to indicate if speed boost is active")]
        public bool isSpeedActivated = false;

        [Tooltip("Flag to indicate if damage boost is active")]
        public bool isDamageActivated = false;

        [Space(10)]
        [Header("Bomb System")]
        [Tooltip("Spawn point for bomb objects")]
        [SerializeField] private GameObject bombSpawn;

        [Space(10)]
        [Header("Advertisement Settings")]
        [Tooltip("Configure when ads are shown after level completion")]
        public AdsAfterLevel adsAfterLevel;

        [Tooltip("The level from which interstitial ads will start showing (Remote Controlled)")]
        public int startLevelShowInter = 1;

        public enum AdsAfterLevel
        {
            NO_ADS, // No ads after levels
            AFTER_EVERY_LEVEL, // Ads after each level
            AFTER_EVERY_TWO_LEVEL, // Ads after every two levels
            AFTER_EVERY_THREE_LEVELS // Ads after every three levels
            
        }

        // Method to spawn bombs
        public void SpawnBombWaveController()
        {
            FindObjectOfType<WaveController>().SpawnBomb(bombSpawn); // Spawn a bomb at the spawn point
        }

        // Updates wave count on UI
        public void WaveCounterUI(int currentWave, int totalWave)
        {
            waveUI.text = currentWave + " / " + totalWave;
        }

        // Called when the script instance is loaded
        private void Awake()
        {
            if(instance == null)
                instance = this; // Set this as the singleton instance
            else
                Destroy(this); // Destroy duplicate instances
        }

        // Initial setup when the game starts
        private void Start()
        {
            levelNo = PlayerPrefs.GetInt("level", 1); // Load saved level number
            if (levelText)
                levelText.text = "Level " + (SceneManager.GetActiveScene().buildIndex - 2); // Update level text

            if(isRewardedScene) // Check if the scene has rewards
            {
                // Auto-generate rewardKey if not assigned
                if (string.IsNullOrEmpty(rewardKey))
                    rewardKey = SceneManager.GetActiveScene().name + "Reward";

                if(PlayerPrefs.GetInt(rewardKey, 0) == 1 && rewardSystem != null) // Reward player if eligible
                {
                    rewardSystem.Rewarded();
                }
                else
                {
                    // Ẩn bảng Reward đối với người mới chơi (TutorialDone = 0) ở màn đầu tiên
                    if (PlayerPrefs.GetInt("TutorialDone", 0) == 0)
                    {
                        RewardDisabled();
                    }
                    else
                    {
                        RewardEnabled(); // Show reward screen if needed
                    }
                }
            }

            if (gamePlatform == GamePlatform.MOBILE) // Setting Reference Resolution for Mobile
                GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
            else // Setting Reference Resolution for PC
                GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);

            if (fireworksObject != null)
                fireworksObject.SetActive(false);

            AddCoins(0); // Initialize coin display
        }

        // Show incoming wave UI and disable it after a delay
        public void EnableIncomingWave()
        {
            incomingWave.SetActive(true);
            StartCoroutine(DisableIncomingWave());
        }

        // Coroutine to disable incoming wave UI after 4 seconds
        IEnumerator DisableIncomingWave()
        {
            yield return new WaitForSeconds(4f);
            _disableIncomingWave();
        }

        // Disable incoming wave UI
        public void _disableIncomingWave()
        {
            incomingWave.SetActive(false);
        }

        // Start the level
        public void Play()
        {
            // Block navigation if Settings panel is open
            if (MainMenuManager.IsSettingsOpen) return;

            if (AudioManager.Instance != null)
                AudioManager.Instance.Play( "ButtonClick" );
            else
                Debug.LogWarning("AudioManager.Instance is missing. Sound not played.");

            Debug.Log("GameManager.Play() triggered. Loading level...");

            // levelNo tracks the Level Number (1, 2, 3...). 
            // Level 1 is at Build Index 3 (due to AdsSplash, Loading, and MainMenu before it).
            int sceneToLoad = levelNo + 2;
            
            // maxSceneIndex is totalNoOfLevels + 2 (Ads, Loading, Menu + Levels)
            int maxSceneIndex = totalNoOfLevels + 2;

            if (sceneToLoad <= maxSceneIndex)
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                // If player has surpassed the max levels, load a random level (from Scene 3 to maxSceneIndex)
                int randLevel = Random.Range(3, maxSceneIndex + 1);
                SceneManager.LoadScene(randLevel);
            }
        }

        // Enable reward screen UI
        public void RewardEnabled()
        {
            Time.timeScale = 0f; // Đóng băng game khi bảng chọn lính/quà hiện lên
            if (drawPad != null) drawPad.enabled = false;
            if (drawPadArea != null) drawPadArea.SetActive(false);
            if (waveController != null) waveController.SetActive(false);
            if (rewardScreen != null) rewardScreen.SetActive(true);
        }

        // Disable reward screen UI and resume game elements
        public void RewardDisabled()
        {
            Time.timeScale = 1f; // Tiếp tục game khi đóng bảng chọn lính/quà
            if (drawPad != null) drawPad.enabled = true;
            if (drawPadArea != null) drawPadArea.SetActive(true);
            if (waveController != null) waveController.SetActive(true);
            if (rewardScreen != null) rewardScreen.SetActive(false);
        }

        // Show rewarded ad for skin reward
        public void PlayRewardedAd() 
        { 
            AudioManager.Instance.Play( "ButtonClick" );

            TheLegends.Base.Ads.AdsManager.Instance.ShowRewarded(TheLegends.Base.Ads.PlacementOrder.One, "reward_skin", () => {
                rewardSystem.Rewarded();
            });
        }

        // Handle mission failure, show retry UI, and disable other elements
        public void missionFailed()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play( "WinAndFailScreenPop" );

            CheckToShowAds();
            if (drawPad != null) drawPad.enabled = false;
            if (shopSystem != null) shopSystem.CloseShopWindow();
            if (shopButton != null) shopButton.SetActive(false);
            // Keep pause button visible when mission fails
            // if (pauseButton != null) pauseButton.SetActive(false);
            if (drawPadArea != null) drawPadArea.SetActive(false);
            // Bỏ ẩn moneyUI để tiền luôn hiện khi thua
            // if (moneyUI != null) moneyUI.SetActive(false);
            // Keep level text visible when mission fails
            // if (levelText != null) levelText.gameObject.SetActive(false);
            if (retryLevel != null) retryLevel.SetActive(true);
            if (retryLevelButton != null) retryLevelButton.SetActive(true);
            if (touchSliderObj != null) touchSliderObj.SetActive(false);
        }

        // Handle mission success, show next level UI, and disable other elements
        public void missionAccomplished()
        {
            if(PlayerPrefs.GetInt("TutorialDone", 0) == 0) 
                PlayerPrefs.SetInt("TutorialDone", 1); // Mark tutorial as done

            if (AudioManager.Instance != null)
                AudioManager.Instance.Play( "WinAndFailScreenPop" );
            
            Time.timeScale = 0f; // Dừng thời gian game khi bảng Victory hiện lên

            CheckToShowAds();
            if (drawPad != null) drawPad.enabled = false;
            if (shopSystem != null) shopSystem.CloseShopWindow();
            if (shopButton != null) shopButton.SetActive(false);
            // Keep pause button visible when mission is accomplished
            // if (pauseButton != null) pauseButton.SetActive(false);
            if (drawPadArea != null) drawPadArea.SetActive(false);
            // Bỏ ẩn moneyUI để tiền luôn hiện khi thắng
            // if (moneyUI != null) moneyUI.SetActive(false);
            // Keep level text visible when mission is accomplished
            // if (levelText != null) levelText.gameObject.SetActive(false);
            if (nextLevel != null) nextLevel.SetActive(true);
            if (nextLevelButton != null) nextLevelButton.SetActive(true);
            if (touchSliderObj != null) touchSliderObj.SetActive(false);

            // Hiện pháo hoa chúc mừng chiến thắng
            if (fireworksObject != null)
            {
                fireworksObject.SetActive(true);
            }

            // Update the total money text on the Victory Panel if assigned
            if (totalMoneyVictoryText != null)
            {
                totalMoneyVictoryText.text = PlayerPrefs.GetInt("Coins", 0).ToString();
            }

            // Cập nhật số tiền kiếm được ở level vừa rồi lên bảng Victory
            if (baseCoinVictoryText != null)
            {
                baseCoinVictoryText.gameObject.SetActive(true); // Đảm bảo text này luôn hiện lại khi bắt đầu bảng Victory
                baseCoinVictoryText.text = coinEarnedThisLevel.ToString();
            }

            // Gửi dữ liệu tiền cho RewardMultiplier để nó tự động nhảy số
            if (rewardMultiplier != null)
            {
                rewardMultiplier.baseCoins = coinEarnedThisLevel;
                
                // Nếu chưa có dynamicCoinText thì gán tạm bằng coinEarnedThisLevelText
                if (rewardMultiplier.dynamicCoinText == null)
                    rewardMultiplier.dynamicCoinText = coinEarnedThisLevelText;
                    
                // Cập nhật lần đầu tiên để hiện số tiền base thay vì nhảy luôn lên x5
                if (rewardMultiplier.dynamicCoinText != null)
                {
                    rewardMultiplier.dynamicCoinText.text = coinEarnedThisLevel.ToString();
                }
            }
        }

        // Retry the current level
        public void Retry()
        {
            Time.timeScale = 1f;
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play( "ButtonClick" );
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // Load the next level or a random one if all levels completed
        public void Next()
        {
            Time.timeScale = 1f;
            levelNo++;
            PlayerPrefs.SetInt("level", levelNo);

            // Level 1 is index 3, so Level X is index X + 2
            int sceneToLoad = levelNo + 2;
            int maxSceneIndex = totalNoOfLevels + 2;

            if (levelNo <= totalNoOfLevels)
                SceneManager.LoadScene(sceneToLoad);
            else
            {
                label:
                int randLevel = Random.Range(3, maxSceneIndex + 1);
                if (randLevel != SceneManager.GetActiveScene().buildIndex)
                    SceneManager.LoadScene(randLevel);
                else
                    goto label;
            }
        }

        // Show ads only once per level based on settings
        private bool callAdOnlyOnce = true;

        private void CheckToShowAds()
        {
            if (!callAdOnlyOnce || adsAfterLevel == AdsAfterLevel.NO_ADS)
                return;

            // Fetch the start level from Remote Config
            startLevelShowInter = FirebaseManager.Instance.RemoteGetValueInt("startLevelInter", startLevelShowInter);

            // Check if current level is eligible for ads
            // Level 1 (Index 3) -> currentLevel = 1
            int currentLevel = SceneManager.GetActiveScene().buildIndex - 2; 
            if (currentLevel < startLevelShowInter)
            {
                Debug.Log($"Ads skipped: Current Level {currentLevel} < Start Level {startLevelShowInter}");
                return;
            }

            if (PlayerPrefs.GetInt("AD", 0) >= ((int)adsAfterLevel - 1))
            {
               TheLegends.Base.Ads.AdsManager.Instance.ShowInterstitial(TheLegends.Base.Ads.AdsType.Interstitial, TheLegends.Base.Ads.PlacementOrder.One, "inter_level");

                PlayerPrefs.SetInt("AD", 0);
            }
            else
            {
                PlayerPrefs.SetInt("AD", PlayerPrefs.GetInt("AD", 0) + 1);
            }

            callAdOnlyOnce = false;
        }

        // Play rewarded video ad to skip level
        public void SkipLvl() 
        { 
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play( "ButtonClick" );
            
            TheLegends.Base.Ads.AdsManager.Instance.ShowRewarded(TheLegends.Base.Ads.PlacementOrder.One, "skip_level", () => {
                Next();
            });
        }

        [Header("Reward Multiplier System")]
        [Tooltip("The UI panel for the multiplier reward")]
        public GameObject multiplierPanel;

        [Tooltip("Reference to the RewardMultiplier script")]
        public RewardMultiplier rewardMultiplier;

        [Tooltip("The green button that starts the ad")]
        public GameObject adsButton;

        [Tooltip("The button that stops the multiplier (initially hidden)")]
        public GameObject stopButton;

        // 1. Skip reward and move to next level
        public void NoThanks()
        {
            Time.timeScale = 1f;
            if (multiplierPanel != null) multiplierPanel.SetActive(false);
            Next();
        }

        // 2. Watch ad to start the multiplier movement
        public void StartMultiplierAds()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play("ButtonClick");

#if UNITY_EDITOR
            Debug.Log("Bypassing Ads in Unity Editor...");
            if (rewardMultiplier != null)
            {
                rewardMultiplier.StartMoving(); // Triangle starts moving
            }
            
            // Swap buttons
            if (adsButton != null) adsButton.SetActive(false);
            if (stopButton != null) stopButton.SetActive(true);

            // Ẩn base coin đi khi bắt đầu quay
            if (baseCoinVictoryText != null) baseCoinVictoryText.gameObject.SetActive(false);
            return;
#endif

            TheLegends.Base.Ads.AdsManager.Instance.ShowRewarded(TheLegends.Base.Ads.PlacementOrder.One, "start_multiplier", () => {
                if (rewardMultiplier != null)
                {
                    rewardMultiplier.StartMoving(); // Triangle starts moving
                }
                
                // Swap buttons
                if (adsButton != null) adsButton.SetActive(false);
                if (stopButton != null) stopButton.SetActive(true);

                // Ẩn base coin đi khi bắt đầu quay
                if (baseCoinVictoryText != null) baseCoinVictoryText.gameObject.SetActive(false);
            });
        }

        // 3. Stop the movement and claim the final reward
        public void StopAndClaim()
        {
            if (rewardMultiplier == null) return;

            // Prevent spam clicking
            if (stopButton != null) stopButton.SetActive(false);

            // Stop the indicator and get multiplier
            rewardMultiplier.StopMoving();
            int multiplier = rewardMultiplier.GetMultiplier();

            if (AudioManager.Instance != null)
                AudioManager.Instance.Play("ButtonClick");

            // Calculate bonus
            int bonusAmount = coinEarnedThisLevel * (multiplier - 1);
            AddCoins(bonusAmount);

            Debug.Log($"Final Reward Multiplier: x{multiplier}. Bonus: {bonusAmount}");

            // Delay loading the next level so the player can see their multiplied money
            StartCoroutine(WaitAndNext());
        }

        private System.Collections.IEnumerator WaitAndNext()
        {
            // Dùng WaitForSecondsRealtime vì Time.timeScale đang bằng 0
            yield return new WaitForSecondsRealtime(1.5f);
            Time.timeScale = 1f;
            Next();
        }

        // Add coins to player's total and update UI
        public void AddCoins(int amountOfCoins)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play( "CoinAdded" );
            PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins", 0) + amountOfCoins);
            coinText.text = PlayerPrefs.GetInt("Coins", 0).ToString();
            coinEarnedThisLevel += amountOfCoins;
            coinEarnedThisLevelText.text = "+ " + coinEarnedThisLevel.ToString();
        }

        // Pause the game and show the pause menu
        public void Pause()
        {
            Time.timeScale = 0f;
            Debug.Log("Game Paused!");
            
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play( "ButtonClick" );

            if (pauseWindow != null) 
            {
                pauseWindow.SetActive(true);
                Debug.Log("Pause Window Activated. Is it active in hierarchy? " + pauseWindow.activeInHierarchy);
            }
            else
            {
                Debug.LogError("Pause Window is NULL in GameManager! Please assign it in the Inspector.");
            }

            if (shopButton != null) shopButton.SetActive(false);
        }

        // Resume the game and hide the pause menu
        public void Resume()
        {
            Time.timeScale = 1f;
            Debug.Log("Game Resumed!");

            if (AudioManager.Instance != null)
                AudioManager.Instance.Play( "ButtonClick" );

            if (pauseWindow != null) 
            {
                pauseWindow.SetActive(false);
            }
            else
            {
                Debug.LogError("Pause Window is NULL in GameManager! Cannot close it.");
            }

            if (shopButton != null) shopButton.SetActive(true);
        }

        // Enable or disable the shop button interactability
        public void SetShopInteractable(bool interactable)
        {
            if (shopButton != null)
            {
                Button btn = shopButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = interactable;
                }
                else
                {
                    shopButton.SetActive(interactable);
                }
            }
        }

        // Restart the current level
        public void Restart()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play( "ButtonClick" );
            pauseWindow.SetActive(false);
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
