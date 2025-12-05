using UnityEngine;
using System.Collections;
using DreamingSlayer.Core;

namespace DreamingSlayer.Enemies
{
    /// <summary>
    /// Spawns enemies from the edges of the screen
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject[] enemyPrefabs;
        [SerializeField] private float baseSpawnInterval = 2f;
        [SerializeField] private float minSpawnInterval = 0.3f;
        [SerializeField] private int baseEnemiesPerWave = 5;
        [SerializeField] private float spawnDistanceFromCenter = 12f;

        [Header("Wave Scaling")]
        [SerializeField] private float healthScalePerWave = 1.2f;
        [SerializeField] private float speedScalePerWave = 1.05f;
        [SerializeField] private float goldScalePerWave = 1.1f;
        [SerializeField] private int extraEnemiesPerWave = 2;

        [Header("Base Enemy Stats")]
        [SerializeField] private float baseEnemyHealth = 50f;
        [SerializeField] private float baseEnemySpeed = 2f;
        [SerializeField] private int baseGoldReward = 10;

        private Camera mainCamera;
        private bool isSpawning;
        private int enemiesLeftToSpawn;
        private Coroutine spawnCoroutine;

        private void Start()
        {
            mainCamera = Camera.main;

            // Subscribe to wave events
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveStarted += OnWaveStarted;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnWaveStarted -= OnWaveStarted;
            }
        }

        private void OnWaveStarted(int wave)
        {
            // Stop any existing spawn routine
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
            }

            // Calculate enemies for this wave
            enemiesLeftToSpawn = baseEnemiesPerWave + (wave - 1) * extraEnemiesPerWave;
            
            // Start spawning
            spawnCoroutine = StartCoroutine(SpawnWave(wave));
        }

        private IEnumerator SpawnWave(int wave)
        {
            isSpawning = true;

            // Calculate spawn interval (gets faster each wave)
            float spawnInterval = Mathf.Max(
                minSpawnInterval, 
                baseSpawnInterval - (wave - 1) * 0.1f
            );

            // Calculate scaled stats for this wave
            float waveMult = Mathf.Pow(1f, wave - 1); // Base multiplier
            float scaledHealth = baseEnemyHealth * Mathf.Pow(healthScalePerWave, wave - 1);
            float scaledSpeed = baseEnemySpeed * Mathf.Pow(speedScalePerWave, wave - 1);
            int scaledGold = Mathf.RoundToInt(baseGoldReward * Mathf.Pow(goldScalePerWave, wave - 1));

            Debug.Log($"Wave {wave}: Spawning {enemiesLeftToSpawn} enemies (HP: {scaledHealth:F0}, Speed: {scaledSpeed:F1}, Gold: {scaledGold})");

            while (enemiesLeftToSpawn > 0)
            {
                SpawnEnemy(scaledHealth, scaledSpeed, scaledGold);
                enemiesLeftToSpawn--;
                
                yield return new WaitForSeconds(spawnInterval);
            }

            isSpawning = false;
        }

        private void SpawnEnemy(float health, float speed, int gold)
        {
            if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

            // Pick random enemy prefab
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            
            // Get spawn position at screen edge
            Vector3 spawnPos = GetRandomSpawnPosition();
            
            // Spawn enemy
            GameObject enemyObj = Instantiate(prefab, spawnPos, Quaternion.identity);
            
            // Configure enemy stats
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.Configure(health, speed, gold);
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            // Random angle around the center
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            
            // Position at spawn distance
            Vector3 pos = new Vector3(
                Mathf.Cos(angle) * spawnDistanceFromCenter,
                Mathf.Sin(angle) * spawnDistanceFromCenter,
                0f
            );

            return pos;
        }

        // Alternative: Spawn from screen edges
        private Vector3 GetSpawnPositionFromScreenEdge()
        {
            if (mainCamera == null) return Vector3.zero;

            // Pick random edge (0=top, 1=right, 2=bottom, 3=left)
            int edge = Random.Range(0, 4);
            
            float x = 0, y = 0;
            Vector3 viewportPos = Vector3.zero;

            switch (edge)
            {
                case 0: // Top
                    viewportPos = new Vector3(Random.Range(0f, 1f), 1.1f, 0);
                    break;
                case 1: // Right
                    viewportPos = new Vector3(1.1f, Random.Range(0f, 1f), 0);
                    break;
                case 2: // Bottom
                    viewportPos = new Vector3(Random.Range(0f, 1f), -0.1f, 0);
                    break;
                case 3: // Left
                    viewportPos = new Vector3(-0.1f, Random.Range(0f, 1f), 0);
                    break;
            }

            Vector3 worldPos = mainCamera.ViewportToWorldPoint(viewportPos);
            worldPos.z = 0;
            
            return worldPos;
        }
    }
}

