using UnityEngine;
using UnityEngine.UI;
using IndianOceanAssets.SlingMaster;

namespace IndianOceanAssets.BridgeSiege
{
    /// <summary>
    /// Manages the Settings panel on the Main Menu.
    /// Handles music and vibration on/off with two separate buttons, checkmarks, and status images.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Settings Panel")]
        [Tooltip("The Settings panel GameObject (will be toggled on/off)")]
        [SerializeField] private GameObject settingsPanel;

    [Header("Music Buttons")]
    [Tooltip("Checkmark GameObject inside the 'On Music' button (enable/disable to show tick)")]
    [SerializeField] private GameObject onMusicCheckmark;

    [Tooltip("Checkmark GameObject inside the 'Off Music' button (enable/disable to show tick)")]
    [SerializeField] private GameObject offMusicCheckmark;

    [Header("Music Status Images")]
    [Tooltip("Image/GameObject displayed when music is ON (e.g. a speaker-on notification)")]
    [SerializeField] private GameObject musicOnStatusImage;

    [Tooltip("Image/GameObject displayed when music is OFF (e.g. a speaker-off notification)")]
    [SerializeField] private GameObject musicOffStatusImage;

    [Header("Vibration Buttons")]
    [Tooltip("Checkmark GameObject inside the 'On Vibration' button (enable/disable to show tick)")]
    [SerializeField] private GameObject onVibrationCheckmark;

    [Tooltip("Checkmark GameObject inside the 'Off Vibration' button (enable/disable to show tick)")]
    [SerializeField] private GameObject offVibrationCheckmark;

    [Header("Vibration Status Images")]
    [Tooltip("Image/GameObject displayed when vibration is ON")]
    [SerializeField] private GameObject vibrationOnStatusImage;

    [Tooltip("Image/GameObject displayed when vibration is OFF")]
    [SerializeField] private GameObject vibrationOffStatusImage;

    private bool isMusicOn;
    private bool isVibrationOn;

    /// <summary>
    /// Public static property so other scripts can check vibration state.
    /// Usage: if (MainMenuManager.IsVibrationOn) Handheld.Vibrate();
    /// </summary>
    public static bool IsVibrationOn { get; internal set; }

    /// <summary>
    /// True when the settings panel is currently open.
    /// Other scripts can check: if (MainMenuManager.IsSettingsOpen) return;
    /// </summary>
    public static bool IsSettingsOpen { get; private set; }

    private void Start()
    {
        // Load saved music setting (default: on)
        isMusicOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        AudioListener.volume = isMusicOn ? 1f : 0f;
        UpdateMusicUI();

        // Load saved vibration setting (default: on)
        isVibrationOn = PlayerPrefs.GetInt("VibrationOn", 1) == 1;
        IsVibrationOn = isVibrationOn;
        UpdateVibrationUI();

        // Ensure settings panel is closed on start
        IsSettingsOpen = false;
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // ---------- Play Game ----------

    /// <summary>
    /// Loads the game scene based on saved level progress.
    /// Assign this to the Play button's OnClick event.
    /// </summary>
    public void Play()
    {
        if (IsSettingsOpen) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.Play("ButtonClick");

        int levelNo = PlayerPrefs.GetInt("level", 1);
        int totalScenes = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
        
        // Level 1 starts at index 3
        int sceneToLoad = levelNo + 2;

        // If the calculated index exceeds the available scenes
        if (sceneToLoad >= totalScenes)
        {
            // Fallback to a random level index between 3 and the last scene
            int minLevelIndex = 3;
            int maxLevelIndex = totalScenes - 1;
            sceneToLoad = UnityEngine.Random.Range(minLevelIndex, maxLevelIndex + 1);
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
    }

    // ---------- Settings Panel ----------

    /// <summary>
    /// Opens the settings panel.
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            IsSettingsOpen = true;
        }
    }

    /// <summary>
    /// Closes the settings panel.
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            IsSettingsOpen = false;
        }
    }

    // ---------- Music On / Off Buttons ----------

    /// <summary>
    /// Called when the player presses the "On Music" button.
    /// Turns music ON, shows checkmark on this button, hides checkmark on Off button.
    /// Shows the "music on" status image, hides the "music off" status image.
    /// </summary>
    public void OnMusicButtonPressed()
    {
        isMusicOn = true;
        AudioListener.volume = 1f;
        PlayerPrefs.SetInt("SoundOn", 1);
        PlayerPrefs.Save();
        UpdateMusicUI();
    }

    /// <summary>
    /// Called when the player presses the "Off Music" button.
    /// Turns music OFF, shows checkmark on this button, hides checkmark on On button.
    /// Shows the "music off" status image, hides the "music on" status image.
    /// </summary>
    public void OffMusicButtonPressed()
    {
        isMusicOn = false;
        AudioListener.volume = 0f;
        PlayerPrefs.SetInt("SoundOn", 0);
        PlayerPrefs.Save();
        UpdateMusicUI();
    }

    /// <summary>
    /// Updates checkmarks and status images based on current music state.
    /// </summary>
    private void UpdateMusicUI()
    {
        // Checkmarks: show tick on the active button, hide on the other
        if (onMusicCheckmark != null)
            onMusicCheckmark.SetActive(isMusicOn);

        if (offMusicCheckmark != null)
            offMusicCheckmark.SetActive(!isMusicOn);

        // Status images: show the matching notification image
        if (musicOnStatusImage != null)
            musicOnStatusImage.SetActive(isMusicOn);

        if (musicOffStatusImage != null)
            musicOffStatusImage.SetActive(!isMusicOn);
    }

    // ---------- Vibration On / Off Buttons ----------

    /// <summary>
    /// Called when the player presses the "On Vibration" button.
    /// </summary>
    public void OnVibrationButtonPressed()
    {
        isVibrationOn = true;
        IsVibrationOn = true;
        PlayerPrefs.SetInt("VibrationOn", 1);
        PlayerPrefs.Save();
        UpdateVibrationUI();
    }

    /// <summary>
    /// Called when the player presses the "Off Vibration" button.
    /// </summary>
    public void OffVibrationButtonPressed()
    {
        isVibrationOn = false;
        IsVibrationOn = false;
        PlayerPrefs.SetInt("VibrationOn", 0);
        PlayerPrefs.Save();
        UpdateVibrationUI();
    }

    /// <summary>
    /// Updates checkmarks and status images based on current vibration state.
    /// </summary>
    private void UpdateVibrationUI()
    {
        if (onVibrationCheckmark != null)
            onVibrationCheckmark.SetActive(isVibrationOn);

        if (offVibrationCheckmark != null)
            offVibrationCheckmark.SetActive(!isVibrationOn);

        if (vibrationOnStatusImage != null)
            vibrationOnStatusImage.SetActive(isVibrationOn);

        if (vibrationOffStatusImage != null)
            vibrationOffStatusImage.SetActive(!isVibrationOn);
    }
}
}
