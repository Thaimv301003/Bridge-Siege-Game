using IndianOceanAssets.SlingMaster;
using UnityEngine;

namespace IndianOceanAssets.BridgeSiege
{
    public class RewardSystem : MonoBehaviour
    {
        // ---------- Object Poolers for Reward Status ----------
        
        [Header("Object Poolers")]
        
        [Tooltip("Object pooler for regular game objects (when reward ad is not claimed)")]
        [SerializeField] private GameObject normalObjectPooler; // Pooler for regular objects
        
        [Tooltip("Object pooler for rewarded game objects (when reward ad is claimed)")]
        [SerializeField] private GameObject rewardedObjectPooler; // Pooler for rewarded objects

        // ---------- Player Settings ----------
        
        [Header("Player Settings")]
        
        [Tooltip("The main player character")]
        [SerializeField] private GameObject playerCharacter; // Reference to the player character

        // ---------- Reward Claim Methods ----------

        // Called when the player claims a reward (e.g., from a rewarded ad)
        public void Rewarded()
        {
            // Activate the pooler for rewarded objects
            if (rewardedObjectPooler != null)
                rewardedObjectPooler.SetActive(true);

            // Set the DrawPad's spawn soldier to the player character (enhanced state)
            if (DrawPad.instance != null)
                DrawPad.instance.spawnSoldier = playerCharacter;

            // Disable the reward screen via the GameManager
            if (GameManager.instance != null)
                GameManager.instance.RewardDisabled();

            // Save reward claim status using a unique reward key
            if (GameManager.instance != null)
                PlayerPrefs.SetInt(GameManager.instance.rewardKey, 1);
        }

        // Called when the player does not claim a reward
        public void NotRewarded()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.Play( "ButtonClick" );

            // Activate the normal object pooler for regular gameplay
            if (normalObjectPooler != null)
                normalObjectPooler.SetActive(true);

            // Disable the reward screen via the GameManager
            if (GameManager.instance != null)
                GameManager.instance.RewardDisabled();
        }
    }
}
