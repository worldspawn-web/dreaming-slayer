using UnityEngine;
using System;
using System.Collections.Generic;
using DreamingSlayer.Defender;

namespace DreamingSlayer.Economy
{
    /// <summary>
    /// Manages all upgrades - levels, costs, and applying effects
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [System.Serializable]
        public class UpgradeConfig
        {
            public UpgradeType type;
            public string displayName;
            public string description;
            public float baseValue;        // Starting bonus at level 1
            public float valuePerLevel;    // Additional bonus per level
            public int baseCost;           // Cost for level 1
            public float costMultiplier;   // Cost multiplier per level
            public int maxLevel;           // Maximum upgrade level
            public Sprite icon;
        }

        public enum UpgradeType
        {
            FireRate,
            Damage,
            Health,
            GoldBonus,
            BulletSpeed
        }

        [Header("Upgrade Configurations")]
        [SerializeField] private List<UpgradeConfig> upgradeConfigs = new List<UpgradeConfig>();

        // Current levels for each upgrade
        private Dictionary<UpgradeType, int> upgradeLevels = new Dictionary<UpgradeType, int>();

        // Events
        public event Action<UpgradeType, int> OnUpgradePurchased; // type, new level
        public event Action OnUpgradesChanged;

        // Gold bonus multiplier (affected by GoldBonus upgrade)
        public float GoldMultiplier { get; private set; } = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeUpgrades();
        }

        private void InitializeUpgrades()
        {
            // Initialize all upgrade levels to 0
            foreach (UpgradeType type in Enum.GetValues(typeof(UpgradeType)))
            {
                upgradeLevels[type] = 0;
            }

            // Set default configs if none defined
            if (upgradeConfigs.Count == 0)
            {
                SetDefaultConfigs();
            }
        }

        private void SetDefaultConfigs()
        {
            upgradeConfigs = new List<UpgradeConfig>
            {
                new UpgradeConfig
                {
                    type = UpgradeType.Damage,
                    displayName = "Damage",
                    description = "Increase bullet damage by {0}",
                    baseValue = 5f,
                    valuePerLevel = 3f,
                    baseCost = 50,
                    costMultiplier = 1.4f,
                    maxLevel = 50
                },
                new UpgradeConfig
                {
                    type = UpgradeType.FireRate,
                    displayName = "Fire Rate",
                    description = "Increase fire rate by {0}/s",
                    baseValue = 0.3f,
                    valuePerLevel = 0.2f,
                    baseCost = 75,
                    costMultiplier = 1.5f,
                    maxLevel = 30
                },
                new UpgradeConfig
                {
                    type = UpgradeType.Health,
                    displayName = "Max Health",
                    description = "Increase max HP by {0}",
                    baseValue = 20f,
                    valuePerLevel = 15f,
                    baseCost = 100,
                    costMultiplier = 1.3f,
                    maxLevel = 50
                },
                new UpgradeConfig
                {
                    type = UpgradeType.GoldBonus,
                    displayName = "Gold Bonus",
                    description = "Increase gold earned by {0}%",
                    baseValue = 10f,
                    valuePerLevel = 5f,
                    baseCost = 150,
                    costMultiplier = 1.6f,
                    maxLevel = 25
                },
                new UpgradeConfig
                {
                    type = UpgradeType.BulletSpeed,
                    displayName = "Bullet Speed",
                    description = "Increase projectile speed by {0}",
                    baseValue = 2f,
                    valuePerLevel = 1.5f,
                    baseCost = 60,
                    costMultiplier = 1.35f,
                    maxLevel = 20
                }
            };
        }

        public UpgradeConfig GetConfig(UpgradeType type)
        {
            return upgradeConfigs.Find(c => c.type == type);
        }

        public List<UpgradeConfig> GetAllConfigs()
        {
            return upgradeConfigs;
        }

        public int GetLevel(UpgradeType type)
        {
            return upgradeLevels.TryGetValue(type, out int level) ? level : 0;
        }

        public int GetNextCost(UpgradeType type)
        {
            var config = GetConfig(type);
            if (config == null) return int.MaxValue;

            int currentLevel = GetLevel(type);
            if (currentLevel >= config.maxLevel) return int.MaxValue;

            return Mathf.RoundToInt(config.baseCost * Mathf.Pow(config.costMultiplier, currentLevel));
        }

        public float GetCurrentValue(UpgradeType type)
        {
            var config = GetConfig(type);
            if (config == null) return 0;

            int level = GetLevel(type);
            if (level == 0) return 0;

            return config.baseValue + (config.valuePerLevel * (level - 1));
        }

        public float GetNextValue(UpgradeType type)
        {
            var config = GetConfig(type);
            if (config == null) return 0;

            int nextLevel = GetLevel(type) + 1;
            return config.baseValue + (config.valuePerLevel * (nextLevel - 1));
        }

        public bool CanPurchase(UpgradeType type)
        {
            var config = GetConfig(type);
            if (config == null) return false;

            int currentLevel = GetLevel(type);
            if (currentLevel >= config.maxLevel) return false;

            int cost = GetNextCost(type);
            return GoldManager.Instance != null && GoldManager.Instance.CanAfford(cost);
        }

        public bool IsMaxLevel(UpgradeType type)
        {
            var config = GetConfig(type);
            if (config == null) return true;
            return GetLevel(type) >= config.maxLevel;
        }

        public bool TryPurchaseUpgrade(UpgradeType type)
        {
            if (!CanPurchase(type)) return false;

            int cost = GetNextCost(type);
            
            if (GoldManager.Instance.TrySpendGold(cost))
            {
                upgradeLevels[type]++;
                ApplyUpgrade(type);
                
                OnUpgradePurchased?.Invoke(type, upgradeLevels[type]);
                OnUpgradesChanged?.Invoke();
                
                Debug.Log($"Purchased {type} upgrade! Now level {upgradeLevels[type]}");
                return true;
            }

            return false;
        }

        private void ApplyUpgrade(UpgradeType type)
        {
            float value = GetCurrentValue(type);

            switch (type)
            {
                case UpgradeType.Damage:
                    if (Defender.Defender.Instance != null)
                    {
                        Defender.Defender.Instance.SetBonusDamage(value);
                    }
                    break;

                case UpgradeType.FireRate:
                    if (Defender.Defender.Instance != null)
                    {
                        Defender.Defender.Instance.SetBonusFireRate(value);
                    }
                    break;

                case UpgradeType.Health:
                    var defenderHealth = FindObjectOfType<DefenderHealth>();
                    if (defenderHealth != null)
                    {
                        defenderHealth.SetBonusHealth(value);
                    }
                    break;

                case UpgradeType.GoldBonus:
                    GoldMultiplier = 1f + (value / 100f); // Convert percentage
                    break;

                case UpgradeType.BulletSpeed:
                    if (Defender.Defender.Instance != null)
                    {
                        Defender.Defender.Instance.SetBonusProjectileSpeed(value);
                    }
                    break;
            }
        }

        /// <summary>
        /// Re-apply all upgrades (useful after loading save)
        /// </summary>
        public void ReapplyAllUpgrades()
        {
            foreach (var type in upgradeLevels.Keys)
            {
                if (upgradeLevels[type] > 0)
                {
                    ApplyUpgrade(type);
                }
            }
        }

        // Save/Load
        public void SaveUpgrades()
        {
            foreach (var kvp in upgradeLevels)
            {
                PlayerPrefs.SetInt($"Upgrade_{kvp.Key}", kvp.Value);
            }
            PlayerPrefs.Save();
        }

        public void LoadUpgrades()
        {
            foreach (UpgradeType type in Enum.GetValues(typeof(UpgradeType)))
            {
                upgradeLevels[type] = PlayerPrefs.GetInt($"Upgrade_{type}", 0);
            }
            ReapplyAllUpgrades();
            OnUpgradesChanged?.Invoke();
        }
    }
}

