using UnityEngine;
using System;
using DreamingSlayer.Core;
using DreamingSlayer.Economy;

namespace DreamingSlayer.Enemies
{
    /// <summary>
    /// Base enemy class - moves towards center and has health
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private int goldReward = 10;
        [SerializeField] private int damageToDefender = 10;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color damageFlashColor = Color.red;
        [SerializeField] private float flashDuration = 0.1f;

        private float currentHealth;
        private bool isDead;
        private Transform target; // usually the defender at center
        private Color originalColor;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => isDead;

        public event Action<Enemy> OnDeath;
        public event Action<float, float> OnHealthChanged; // current, max

        private void Start()
        {
            currentHealth = maxHealth;
            
            // Find the defender (center target)
            var defender = FindObjectOfType<DreamingSlayer.Defender.Defender>();
            if (defender != null)
            {
                target = defender.transform;
            }
            else
            {
                // Default to world center if no defender
                target = null;
            }

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            // Register with game manager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterEnemySpawned();
            }
        }

        private void Update()
        {
            if (isDead) return;
            
            MoveTowardsTarget();
        }

        private void MoveTowardsTarget()
        {
            Vector3 targetPos = target != null ? target.position : Vector3.zero;
            Vector3 direction = (targetPos - transform.position).normalized;
            
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

            // Optional: Rotate to face movement direction
            if (direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle - 90f); // -90 if sprite faces up
            }
        }

        public void TakeDamage(float damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // Flash effect
            if (spriteRenderer != null)
            {
                StopAllCoroutines();
                StartCoroutine(DamageFlash());
            }

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private System.Collections.IEnumerator DamageFlash()
        {
            spriteRenderer.color = damageFlashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            // Give gold reward
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.AddGold(goldReward, transform.position);
            }

            // Notify game manager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterEnemyKilled();
            }

            // Invoke death event
            OnDeath?.Invoke(this);

            // TODO: Death animation/particles

            // Destroy the enemy
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if we reached the defender
            var defender = other.GetComponent<DreamingSlayer.Defender.Defender>();
            if (defender != null)
            {
                // Deal damage to defender
                var defenderHealth = other.GetComponent<DreamingSlayer.Defender.DefenderHealth>();
                if (defenderHealth != null)
                {
                    defenderHealth.TakeDamage(damageToDefender);
                }
                
                // Destroy self (don't give gold for reaching defender)
                isDead = true;
                
                // Still notify game manager
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RegisterEnemyKilled();
                }
                
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Configure enemy stats (used by spawner for wave scaling)
        /// </summary>
        public void Configure(float health, float speed, int gold)
        {
            maxHealth = health;
            currentHealth = health;
            moveSpeed = speed;
            goldReward = gold;
        }
    }
}

