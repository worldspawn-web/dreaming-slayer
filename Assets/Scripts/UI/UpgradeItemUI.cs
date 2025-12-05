using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DreamingSlayer.Economy;

namespace DreamingSlayer.UI
{
    /// <summary>
    /// UI for a single upgrade item in the shop
    /// </summary>
    public class UpgradeItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;

        [Header("Colors")]
        [SerializeField] private Color affordableColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color unaffordableColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color maxLevelColor = new Color(1f, 0.84f, 0f);

        private UpgradeManager.UpgradeType upgradeType;
        private UpgradeManager.UpgradeConfig config;

        public void Initialize(UpgradeManager.UpgradeType type)
        {
            upgradeType = type;
            config = UpgradeManager.Instance.GetConfig(type);

            if (config == null)
            {
                Debug.LogError($"UpgradeItemUI: No config found for {type}");
                return;
            }

            // Set name
            if (nameText != null)
            {
                nameText.text = config.displayName;
            }

            // Set icon if available
            if (iconImage != null && config.icon != null)
            {
                iconImage.sprite = config.icon;
            }

            // Setup button
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }

            Refresh();
        }

        public void Refresh()
        {
            if (UpgradeManager.Instance == null) return;

            int currentLevel = UpgradeManager.Instance.GetLevel(upgradeType);
            bool isMaxLevel = UpgradeManager.Instance.IsMaxLevel(upgradeType);
            bool canAfford = UpgradeManager.Instance.CanPurchase(upgradeType);

            // Update level text
            if (levelText != null)
            {
                if (isMaxLevel)
                {
                    levelText.text = $"Lv.{currentLevel} (MAX)";
                }
                else
                {
                    levelText.text = $"Lv.{currentLevel}";
                }
            }

            // Update cost text
            if (costText != null)
            {
                if (isMaxLevel)
                {
                    costText.text = "MAXED";
                    costText.color = maxLevelColor;
                }
                else
                {
                    int cost = UpgradeManager.Instance.GetNextCost(upgradeType);
                    costText.text = $"{cost:N0} G";
                    costText.color = canAfford ? affordableColor : unaffordableColor;
                }
            }

            // Update value text (show current → next value)
            if (valueText != null)
            {
                float currentValue = UpgradeManager.Instance.GetCurrentValue(upgradeType);
                
                if (isMaxLevel)
                {
                    valueText.text = FormatValue(currentValue);
                }
                else
                {
                    float nextValue = UpgradeManager.Instance.GetNextValue(upgradeType);
                    if (currentLevel == 0)
                    {
                        valueText.text = $"→ +{FormatValue(nextValue)}";
                    }
                    else
                    {
                        valueText.text = $"+{FormatValue(currentValue)} → +{FormatValue(nextValue)}";
                    }
                }
            }

            // Update button interactability
            if (purchaseButton != null)
            {
                purchaseButton.interactable = canAfford && !isMaxLevel;
            }

            // Update background color
            if (backgroundImage != null)
            {
                if (isMaxLevel)
                {
                    backgroundImage.color = new Color(maxLevelColor.r, maxLevelColor.g, maxLevelColor.b, 0.3f);
                }
                else if (canAfford)
                {
                    backgroundImage.color = new Color(affordableColor.r, affordableColor.g, affordableColor.b, 0.2f);
                }
                else
                {
                    backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                }
            }
        }

        private string FormatValue(float value)
        {
            // Format based on upgrade type
            if (upgradeType == UpgradeManager.UpgradeType.GoldBonus)
            {
                return $"{value:F0}%";
            }
            else if (upgradeType == UpgradeManager.UpgradeType.FireRate)
            {
                return $"{value:F1}/s";
            }
            else
            {
                return $"{value:F0}";
            }
        }

        private void OnPurchaseClicked()
        {
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.TryPurchaseUpgrade(upgradeType);
                Refresh();
            }
        }
    }
}

