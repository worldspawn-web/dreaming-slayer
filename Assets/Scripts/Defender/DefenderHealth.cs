using UnityEngine;
using System;
using DreamingSlayer.Core;

namespace DreamingSlayer.Defender
{
    /// <summary>
    /// Manages the Defender's health - can be damaged by enemies reaching the center
    /// </summary>
    public class DefenderHealth : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] private float baseMaxHealth = 100f;
        
        [Header("Visual Feedback")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color damageColor = Color.red;
        [SerializeField] private float flashDuration = 0.15f;

        private float bonusHealth;
        private float currentHealth;
        private Color originalColor;
        
        public float MaxHealth => baseMaxHealth + bonusHealth;
        public float CurrentHealth => currentHealth;
        public float HealthPercent => currentHealth / MaxHealth;
        public bool IsDead => currentHealth <= 0;

        public event Action<float, float> OnHealthChanged; // current, max
        public event Action OnDeath;
        public event Action OnDamaged;

        private void Start()
        {
            currentHealth = MaxHealth;
            
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
            
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        }

        public void SetBonusHealth(float bonus)
        {
            float healthPercent = currentHealth / MaxHealth;
            bonusHealth = bonus;
            
            // Maintain same health percentage after upgrade
            currentHealth = MaxHealth * healthPercent;
            
            // But also heal a bit as reward
            Heal(bonus * 0.5f);
            
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
            OnDamaged?.Invoke();

            // Visual feedback
            if (spriteRenderer != null)
            {
                StopAllCoroutines();
                StartCoroutine(DamageFlash());
            }

            Debug.Log($"Defender took {damage} damage! HP: {currentHealth}/{MaxHealth}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (IsDead) return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, MaxHealth);
            
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        }

        public void FullHeal()
        {
            currentHealth = MaxHealth;
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        }

        private System.Collections.IEnumerator DamageFlash()
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }

        private void Die()
        {
            Debug.Log("Defender has been destroyed! Game Over!");
            OnDeath?.Invoke();
            
            // Pause the game
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }
        }

        /// <summary>
        /// Reset health to full (for restarting)
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = MaxHealth;
            OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        }
    }
}

