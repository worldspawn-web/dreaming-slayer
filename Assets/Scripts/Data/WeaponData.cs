using UnityEngine;

namespace DreamingSlayer.Data
{
    /// <summary>
    /// ScriptableObject defining weapon properties
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Dreaming Slayer/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("Basic Info")]
        public string weaponName = "Basic Gun";
        public string description = "A simple weapon.";
        public Sprite icon;
        
        [Header("Combat Stats")]
        public float damage = 10f;
        public float fireRate = 2f; // shots per second
        public float projectileSpeed = 15f;
        public float range = 10f;
        
        [Header("Projectile")]
        public GameObject projectilePrefab;
        public int projectilesPerShot = 1;
        public float spreadAngle = 0f; // for shotgun-style weapons
        public int pierceCount = 0;
        
        [Header("Visual/Audio")]
        public AudioClip fireSound;
        public GameObject muzzleFlashPrefab;
        
        [Header("Unlock")]
        public int unlockCost = 0; // 0 = starting weapon
        public int requiredWave = 1;
        public bool isUnlocked = true;
    }
}

