using UnityEngine;
using UnityEngine.UI;
using DreamingSlayer.Economy;
using System.Collections.Generic;

namespace DreamingSlayer.UI
{
    /// <summary>
    /// Controls the upgrade shop modal window
    /// </summary>
    public class UpgradeShopUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Button openShopButton;
        [SerializeField] private Button closeShopButton;
        [SerializeField] private Transform upgradeItemsContainer;
        [SerializeField] private GameObject upgradeItemPrefab;

        [Header("Settings")]
        [SerializeField] private bool pauseWhenOpen = true;

        private List<UpgradeItemUI> upgradeItems = new List<UpgradeItemUI>();
        private bool isOpen;

        private void Start()
        {
            // Setup button listeners
            if (openShopButton != null)
            {
                openShopButton.onClick.AddListener(OpenShop);
            }

            if (closeShopButton != null)
            {
                closeShopButton.onClick.AddListener(CloseShop);
            }

            // Start with shop closed
            if (shopPanel != null)
            {
                shopPanel.SetActive(false);
            }

            // Create upgrade items
            CreateUpgradeItems();

            // Subscribe to gold changes to update affordability
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.OnGoldChanged += OnGoldChanged;
            }

            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnUpgradesChanged += RefreshAllItems;
            }
        }

        private void OnDestroy()
        {
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.OnGoldChanged -= OnGoldChanged;
            }

            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnUpgradesChanged -= RefreshAllItems;
            }
        }

        private void CreateUpgradeItems()
        {
            if (UpgradeManager.Instance == null || upgradeItemPrefab == null || upgradeItemsContainer == null)
            {
                Debug.LogError("UpgradeShopUI: Missing references!");
                return;
            }

            var configs = UpgradeManager.Instance.GetAllConfigs();

            foreach (var config in configs)
            {
                GameObject itemObj = Instantiate(upgradeItemPrefab, upgradeItemsContainer);
                UpgradeItemUI itemUI = itemObj.GetComponent<UpgradeItemUI>();

                if (itemUI != null)
                {
                    itemUI.Initialize(config.type);
                    upgradeItems.Add(itemUI);
                }
            }
        }

        public void OpenShop()
        {
            if (shopPanel == null) return;

            isOpen = true;
            shopPanel.SetActive(true);
            RefreshAllItems();

            if (pauseWhenOpen)
            {
                Time.timeScale = 0f;
            }
        }

        public void CloseShop()
        {
            if (shopPanel == null) return;

            isOpen = false;
            shopPanel.SetActive(false);

            if (pauseWhenOpen)
            {
                Time.timeScale = 1f;
            }
        }

        public void ToggleShop()
        {
            if (isOpen)
                CloseShop();
            else
                OpenShop();
        }

        private void OnGoldChanged(int gold)
        {
            if (isOpen)
            {
                RefreshAllItems();
            }
        }

        private void RefreshAllItems()
        {
            foreach (var item in upgradeItems)
            {
                item.Refresh();
            }
        }

        private void Update()
        {
            // ESC to close shop
            if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseShop();
            }
        }
    }
}

