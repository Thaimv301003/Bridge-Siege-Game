using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using IndianOceanAssets.SlingMaster;

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

        [Tooltip("Counter for coins earned in the current level")]
        private int coinEarnedThisLevel = 0;

        [Tooltip("Reward screen UI shown after level completion")]
        [SerializeField] private GameObject rewardScreen;

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
            waveUI.text = "Wave " + currentWave + " / " + totalWave;
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
                levelText.text = "Level " + SceneManager.GetActiveScene().buildIndex; // Update level text

            if(isRewardedScene) // Check if the scene has rewards
            {
                if(PlayerPrefs.GetInt(rewardKey, 0) == 1) // Reward player if eligible
                    rewardSystem.Rewarded();
                else
                    RewardEnabled(); // Show reward screen if needed
            }

            if (gamePlatform == GamePlatform.MOBILE) // Setting Reference Resolution for Mobile
                GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
            else // Setting Reference Resolution for PC
                GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);

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

            // levelNo tracks the Build Index. If it's 1 (default), it points to Main Menu. We need it to point to Level 1 (index 2).
            int sceneToLoad = (levelNo <= 1) ? 2 : levelNo;
            
            // Since Scene 2 is Level 1, the max scene index is totalNoOfLevels + 1
            int maxSceneIndex = totalNoOfLevels + 1;

            if (sceneToLoad <= maxSceneIndex)
            {
                SceneManager.LoadScene(sceneToLoad);
            }
            else
            {
                // If player has surpassed the max levels, load a random level (from Scene 2 to maxSceneIndex)
                int randLevel = Random.Range(2, maxSceneIndex + 1);
                SceneManager.LoadScene(randLevel);
            }
        }

        // Enable reward screen UI
        public void RewardEnabled()
        {
            drawPad.enabled = false;
            drawPadArea.SetActive(false);
            waveController.SetActive(false);
            rewardScreen.SetActive(true);
        }

        // Disable reward screen UI and resume game elements
        public void RewardDisabled()
        {
            drawPad.enabled = true;
            drawPadArea.SetActive(true);
            waveController.SetActive(true);
            rewardScreen.SetActive(false);
        }

        // Show rewarded ad for skin reward
        public void PlayRewardedAd() 
        { 
            AudioManager.Instance.Play( "ButtonClick" );

            //AdManager.Instance.isSkin = true; 
            //AdManager.Instance.ShowRewardedVideo(); 
        }

        // Handle mission failure, show retry UI, and disable other elements
        public void missionFailed()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play( "WinAndFailScreenPop" );

            CheckToShowAds();
            drawPad.enabled = false;
            shopSystem.CloseShopWindow();
            shopButton.SetActive(false);
            pauseButton.SetActive(false);
            drawPadArea.SetActive(false);
            moneyUI.SetActive(false);
            levelText.gameObject.SetActive(false);
            retryLevel.SetActive(true);
            retryLevelButton.SetActive(true);
            touchSliderObj.SetActive(false);
        }

        // Handle mission success, show next level UI, and disable other elements
        public void missionAccomplished()
        {
            if(PlayerPrefs.GetInt("TutorialDone", 0) == 0) 
                PlayerPrefs.SetInt("TutorialDone", 1); // Mark tutorial as done

            if (AudioManager.Instance != null)
                AudioManager.Instance.Play( "WinAndFailScreenPop" );
            
            CheckToShowAds();
            drawPad.enabled = false;
            shopSystem.CloseShopWindow();
            shopButton.SetActive(false);
            pauseButton.SetActive(false);
            drawPadArea.SetActive(false);
            moneyUI.SetActive(false);
            levelText.gameObject.SetActive(false);
            nextLevel.SetActive(true);
            nextLevelButton.SetActive(true);
            touchSliderObj.SetActive(false);
        }

        // Retry the current level
        public void Retry()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play( "ButtonClick" );
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        // Load the next level or a random one if all levels completed
        public void Next()
        {
            levelNo++;
            PlayerPrefs.SetInt("level", levelNo);

            if (levelNo < totalNoOfLevels + 1)
                SceneManager.LoadScene(levelNo);
            else
            {
                label:
                int randLevel = Random.Range(3, totalNoOfLevels + 1);
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

            if (PlayerPrefs.GetInt("AD", 0) >= ((int)adsAfterLevel - 1))
            {
                //AdManager.Instance.ShowInterstitial();
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
            //AdManager.Instance.isSkipLevel = true; 
            //AdManager.Instance.ShowRewardedVideo();  
        }

        // Play rewarded ad to double money earned
        public void DoubleMoney()
        {
            //AdManager.Instance.isDoubleMoney = true; 
            //AdManager.Instance.ShowRewardedVideo();
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
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play( "ButtonClick" );
            pauseWindow.SetActive(true);
            shopButton.SetActive(false);
        }

        // Resume the game and hide the pause menu
        public void Resume()
        {
            Time.timeScale = 1f;
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play( "ButtonClick" );
            pauseWindow.SetActive(false);
            shopButton.SetActive(true);
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
