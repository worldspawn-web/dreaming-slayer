using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DreamingSlayer.Defender;

namespace DreamingSlayer.UI
{
    /// <summary>
    /// Displays the Defender's health bar
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private DefenderHealth defenderHealth;

        [Header("Colors")]
        [SerializeField] private Color healthyColor = Color.green;
        [SerializeField] private Color damagedColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;
        [SerializeField] private float damagedThreshold = 0.5f;
        [SerializeField] private float criticalThreshold = 0.25f;

        private void Start()
        {
            // Find DefenderHealth if not assigned
            if (defenderHealth == null)
            {
                defenderHealth = FindObjectOfType<DefenderHealth>();
            }

            if (defenderHealth != null)
            {
                defenderHealth.OnHealthChanged += UpdateHealthBar;
                // Initialize
                UpdateHealthBar(defenderHealth.CurrentHealth, defenderHealth.MaxHealth);
            }
            else
            {
                Debug.LogWarning("HealthBarUI: DefenderHealth not found!");
            }
        }

        private void OnDestroy()
        {
            if (defenderHealth != null)
            {
                defenderHealth.OnHealthChanged -= UpdateHealthBar;
            }
        }

        private void UpdateHealthBar(float current, float max)
        {
            float percent = current / max;

            // Update fill amount
            if (fillImage != null)
            {
                fillImage.fillAmount = percent;

                // Update color based on health
                if (percent <= criticalThreshold)
                {
                    fillImage.color = criticalColor;
                }
                else if (percent <= damagedThreshold)
                {
                    fillImage.color = damagedColor;
                }
                else
                {
                    fillImage.color = healthyColor;
                }
            }

            // Update text
            if (healthText != null)
            {
                healthText.text = $"{Mathf.Ceil(current)}/{Mathf.Ceil(max)}";
            }
        }
    }
}

