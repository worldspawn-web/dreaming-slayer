using UnityEngine;
using DreamingSlayer.Weapons;
using DreamingSlayer.Enemies;

namespace DreamingSlayer.Defender
{
    /// <summary>
    /// The central defender that auto-targets and shoots enemies
    /// </summary>
    public class Defender : MonoBehaviour
    {
        public static Defender Instance { get; private set; }

        [Header("Targeting")]
        [SerializeField] private float targetingRange = 10f;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private TargetingMode targetingMode = TargetingMode.Closest;

        [Header("Weapon")]
        [SerializeField] private float fireRate = 2f; // shots per second
        [SerializeField] private float damage = 10f;
        [SerializeField] private float projectileSpeed = 15f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;

        [Header("Visual")]
        [SerializeField] private Transform turretPivot; // rotates to face target
        [SerializeField] private float rotationSpeed = 10f;

        private Enemy currentTarget;
        private float nextFireTime;

        // Bonus stats from upgrades
        private float bonusDamage;
        private float bonusFireRate;
        private float bonusProjectileSpeed;

        public float TotalDamage => damage + bonusDamage;
        public float TotalFireRate => fireRate + bonusFireRate;
        public float TotalProjectileSpeed => projectileSpeed + bonusProjectileSpeed;
        
        public float Damage => damage;
        public float FireRate => fireRate;
        public float Range => targetingRange;

        public enum TargetingMode
        {
            Closest,
            LowestHealth,
            HighestHealth,
            Random
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Validate setup
            if (enemyLayer == 0)
            {
                Debug.LogWarning("Defender: Enemy Layer is not set! Go to Defender Inspector and set 'Enemy Layer' to the 'Enemy' layer.");
            }
            if (projectilePrefab == null)
            {
                Debug.LogError("Defender: Projectile Prefab is not assigned! Drag a projectile prefab into the Inspector.");
            }
        }

        private void Update()
        {
            FindTarget();
            RotateTowardsTarget();
            TryShoot();
        }

        private void FindTarget()
        {
            // Find all enemies in range
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, targetingRange, enemyLayer);
            
            if (hits.Length == 0)
            {
                currentTarget = null;
                return;
            }

            // Select target based on targeting mode
            currentTarget = SelectTarget(hits);
        }

        private Enemy SelectTarget(Collider2D[] hits)
        {
            Enemy bestTarget = null;
            float bestValue = float.MaxValue;

            foreach (var hit in hits)
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy == null || enemy.IsDead) continue;

                float value = 0f;
                
                switch (targetingMode)
                {
                    case TargetingMode.Closest:
                        value = Vector2.Distance(transform.position, enemy.transform.position);
                        if (value < bestValue)
                        {
                            bestValue = value;
                            bestTarget = enemy;
                        }
                        break;
                        
                    case TargetingMode.LowestHealth:
                        value = enemy.CurrentHealth;
                        if (value < bestValue)
                        {
                            bestValue = value;
                            bestTarget = enemy;
                        }
                        break;
                        
                    case TargetingMode.HighestHealth:
                        value = -enemy.CurrentHealth; // negative so lower is better
                        if (value < bestValue)
                        {
                            bestValue = value;
                            bestTarget = enemy;
                        }
                        break;
                        
                    case TargetingMode.Random:
                        // Just pick the first one (collider order is somewhat random)
                        return enemy;
                }
            }

            return bestTarget;
        }

        private void RotateTowardsTarget()
        {
            if (turretPivot == null) return;
            if (currentTarget == null) return;

            Vector2 direction = (currentTarget.transform.position - turretPivot.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            turretPivot.rotation = Quaternion.Lerp(
                turretPivot.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
        }

        private void TryShoot()
        {
            if (currentTarget == null) return;
            if (Time.time < nextFireTime) return;
            if (projectilePrefab == null) return;

            Shoot();
            nextFireTime = Time.time + (1f / TotalFireRate);
        }

        private void Shoot()
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            Vector2 direction = (currentTarget.transform.position - spawnPos).normalized;

            GameObject projectileObj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Initialize(direction, TotalProjectileSpeed, TotalDamage);
            }
        }

        // Upgrade bonus setters (called by UpgradeManager)
        public void SetBonusDamage(float bonus)
        {
            bonusDamage = bonus;
        }

        public void SetBonusFireRate(float bonus)
        {
            bonusFireRate = bonus;
        }

        public void SetBonusProjectileSpeed(float bonus)
        {
            bonusProjectileSpeed = bonus;
        }

        // Legacy upgrade methods (direct add)
        public void UpgradeDamage(float amount)
        {
            damage += amount;
        }

        public void UpgradeFireRate(float amount)
        {
            fireRate += amount;
        }

        public void UpgradeRange(float amount)
        {
            targetingRange += amount;
        }

        public void SetTargetingMode(TargetingMode mode)
        {
            targetingMode = mode;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw targeting range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, targetingRange);

            // Draw line to current target
            if (currentTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, currentTarget.transform.position);
            }
        }
    }
}


