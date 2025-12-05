using UnityEngine;

namespace DreamingSlayer.Data
{
    /// <summary>
    /// ScriptableObject defining enemy properties
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Dreaming Slayer/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [Header("Basic Info")]
        public string enemyName = "Basic Enemy";
        public Sprite sprite;
        public Color tintColor = Color.white;
        
        [Header("Stats")]
        public float maxHealth = 100f;
        public float moveSpeed = 2f;
        public int goldReward = 10;
        public int damageToDefender = 10;
        
        [Header("Scaling")]
        [Tooltip("Multiplier applied to base stats per wave")]
        public float healthScaling = 1.0f;
        public float speedScaling = 1.0f;
        public float goldScaling = 1.0f;
        
        [Header("Spawn")]
        [Tooltip("Minimum wave this enemy can appear")]
        public int minWave = 1;
        [Tooltip("Relative spawn weight")]
        public float spawnWeight = 1f;
        
        [Header("Special")]
        public bool isBoss = false;
        public bool canFly = false; // for future pathfinding
    }
}

