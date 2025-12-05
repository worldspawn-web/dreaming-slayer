using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DreamingSlayer.Core;
using DreamingSlayer.Economy;

namespace DreamingSlayer.UI
{
    /// <summary>
    /// Main game HUD - displays gold, wave, and stats
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        [Header("Gold Display")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private string goldFormat = "{0:N0}";

        [Header("Wave Display")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private string waveFormat = "Wave {0}";

        [Header("Stats Display")]
        [SerializeField] private TextMeshProUGUI dpsText;
        [SerializeField] private TextMeshProUGUI killsText;

        [Header("Gold Popup")]
        [SerializeField] private GameObject goldPopupPrefab;
        [SerializeField] private Transform popupContainer;

        private int totalKills;

        private void Start()
        {
            // Subscribe to events
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.OnGoldChanged += UpdateGoldDisplay;
                GoldManager.Instance.OnGoldCollected += ShowGoldPopup;
                UpdateGoldDisplay(GoldManager.Instance.CurrentGold);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveStarted += UpdateWaveDisplay;
                UpdateWaveDisplay(GameManager.Instance.CurrentWave);
            }
        }

        private void OnDestroy()
        {
            if (GoldManager.Instance != null)
            {
                GoldManager.Instance.OnGoldChanged -= UpdateGoldDisplay;
                GoldManager.Instance.OnGoldCollected -= ShowGoldPopup;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveStarted -= UpdateWaveDisplay;
            }
        }

        private void UpdateGoldDisplay(int gold)
        {
            if (goldText != null)
            {
                goldText.text = string.Format(goldFormat, gold);
            }
        }

        private void UpdateWaveDisplay(int wave)
        {
            if (waveText != null)
            {
                waveText.text = string.Format(waveFormat, wave);
            }
        }

        public void IncrementKills()
        {
            totalKills++;
            if (killsText != null)
            {
                killsText.text = $"Kills: {totalKills:N0}";
            }
        }

        private void ShowGoldPopup(int amount, Vector3 worldPosition)
        {
            if (goldPopupPrefab == null) return;
            
            // Convert world position to screen position
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            
            // Instantiate popup
            Transform container = popupContainer != null ? popupContainer : transform;
            GameObject popup = Instantiate(goldPopupPrefab, container);
            popup.transform.position = screenPos;

            // Set text if available
            TextMeshProUGUI text = popup.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = $"+{amount}";
            }

            // Auto destroy
            Destroy(popup, 1f);
        }

        public void UpdateDPS(float dps)
        {
            if (dpsText != null)
            {
                dpsText.text = $"DPS: {dps:N0}";
            }
        }
    }
}

