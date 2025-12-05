using UnityEngine;
using System;

namespace DreamingSlayer.Core
{
    /// <summary>
    /// Central game manager - handles game state and core events
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game State")]
        [SerializeField] private bool isPaused;
        
        public bool IsPaused => isPaused;
        public bool IsPlaying => !isPaused;

        // Events for other systems to subscribe to
        public event Action OnGamePaused;
        public event Action OnGameResumed;
        public event Action<int> OnWaveStarted;
        public event Action<int> OnWaveCompleted;

        [Header("Wave System")]
        [SerializeField] private int currentWave = 1;
        public int CurrentWave => currentWave;

        private int enemiesAliveInWave;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            StartWave(1);
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
            OnGamePaused?.Invoke();
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
            OnGameResumed?.Invoke();
        }

        public void TogglePause()
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }

        public void StartWave(int wave)
        {
            currentWave = wave;
            OnWaveStarted?.Invoke(wave);
            Debug.Log($"Wave {wave} started!");
        }

        public void RegisterEnemySpawned()
        {
            enemiesAliveInWave++;
        }

        public void RegisterEnemyKilled()
        {
            enemiesAliveInWave--;
            if (enemiesAliveInWave <= 0)
            {
                CompleteWave();
            }
        }

        private void CompleteWave()
        {
            OnWaveCompleted?.Invoke(currentWave);
            Debug.Log($"Wave {currentWave} completed!");
            
            // Start next wave after a short delay
            Invoke(nameof(StartNextWave), 2f);
        }

        private void StartNextWave()
        {
            StartWave(currentWave + 1);
        }

        public void RestartGame()
        {
            currentWave = 1;
            enemiesAliveInWave = 0;
            Time.timeScale = 1f;
            isPaused = false;
            
            // Reload the scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
    }
}

