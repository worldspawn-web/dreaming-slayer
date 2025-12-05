using UnityEngine;
using System;

namespace DreamingSlayer.Economy
{
    /// <summary>
    /// Manages the player's gold currency
    /// </summary>
    public class GoldManager : MonoBehaviour
    {
        public static GoldManager Instance { get; private set; }

        [Header("Starting Gold")]
        [SerializeField] private int startingGold = 0;
        
        private int currentGold;
        public int CurrentGold => currentGold;

        // Events for UI and other systems
        public event Action<int> OnGoldChanged;
        public event Action<int, Vector3> OnGoldCollected; // amount, world position

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            currentGold = startingGold;
        }

        private void Start()
        {
            OnGoldChanged?.Invoke(currentGold);
        }

        public void AddGold(int amount, Vector3? worldPosition = null)
        {
            if (amount <= 0) return;
            
            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
            
            if (worldPosition.HasValue)
            {
                OnGoldCollected?.Invoke(amount, worldPosition.Value);
            }
            
            Debug.Log($"Gold collected: +{amount}. Total: {currentGold}");
        }

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0) return false;
            if (currentGold < amount) return false;
            
            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            
            Debug.Log($"Gold spent: -{amount}. Total: {currentGold}");
            return true;
        }

        public bool CanAfford(int amount)
        {
            return currentGold >= amount;
        }

        /// <summary>
        /// Save gold to PlayerPrefs (simple persistence)
        /// </summary>
        public void SaveGold()
        {
            PlayerPrefs.SetInt("PlayerGold", currentGold);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load gold from PlayerPrefs
        /// </summary>
        public void LoadGold()
        {
            currentGold = PlayerPrefs.GetInt("PlayerGold", startingGold);
            OnGoldChanged?.Invoke(currentGold);
        }
    }
}

