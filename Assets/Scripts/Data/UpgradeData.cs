using UnityEngine;

namespace DreamingSlayer.Data
{
    /// <summary>
    /// ScriptableObject defining upgrade properties
    /// </summary>
    [CreateAssetMenu(fileName = "NewUpgrade", menuName = "Dreaming Slayer/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
        [Header("Basic Info")]
        public string upgradeName = "Damage Up";
        public string description = "Increases damage by {0}";
        public Sprite icon;
        
        [Header("Upgrade Type")]
        public UpgradeType type = UpgradeType.Damage;
        
        [Header("Values")]
        public float baseValue = 5f; // starting bonus
        public float valuePerLevel = 2f; // additional bonus per level
        public int maxLevel = 10;
        
        [Header("Cost")]
        public int baseCost = 100;
        public float costMultiplierPerLevel = 1.5f; // cost increases each level
        
        public enum UpgradeType
        {
            Damage,
            FireRate,
            Range,
            ProjectileSpeed,
            CritChance,
            CritDamage,
            GoldBonus,
            MaxHealth
        }
        
        /// <summary>
        /// Get the value for a specific level
        /// </summary>
        public float GetValueAtLevel(int level)
        {
            return baseValue + (valuePerLevel * (level - 1));
        }
        
        /// <summary>
        /// Get the cost to upgrade to a specific level
        /// </summary>
        public int GetCostForLevel(int level)
        {
            return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplierPerLevel, level - 1));
        }
        
        /// <summary>
        /// Get formatted description with current value
        /// </summary>
        public string GetDescription(int currentLevel)
        {
            float value = GetValueAtLevel(currentLevel);
            return string.Format(description, value);
        }
    }
}

