using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using PetsWars;

namespace Managers
{
    /// <summary>
    /// Maneja el sistema de oleadas de enemigos
    /// Genera oleadas progresivamente más difíciles con pausas entre ellas
    /// Spawna power-ups cuando los enemigos mueren
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [System.Serializable]
        public class EnemySpawnData
        {
            public GameObject enemyPrefab;
            public int count;
            public float spawnDelay = 0.5f;
        }

        [Header("Wave Settings")]
        [SerializeField] private int currentWave = 0;
        [SerializeField] private float timeBetweenWaves = 10f;
        [SerializeField] private float spawnRadius = 12f;
        [SerializeField] private float spawnDistanceFromPlayer = 8f;

        [Header("Map Bounds")]
        [SerializeField] private bool useMapBounds = true;
        [SerializeField] private float mapMinX = -23f;
        [SerializeField] private float mapMaxX = 23f;
        [SerializeField] private float mapMinY = -23f;
        [SerializeField] private float mapMaxY = 23f;

        [Header("Enemy Prefabs")]
        [SerializeField] private GameObject catPrefab;
        [SerializeField] private GameObject dogPrefab;

        [Header("Power-Up Spawning")]
        [SerializeField] private GameObject[] powerUpPrefabs;
        [SerializeField] private float powerUpSpawnChance = 0.25f; // 25%
        [SerializeField] private bool debugPowerUpSpawn = false;

        [Header("Difficulty Scaling")]
        [SerializeField] private float healthScaling = 1.1f;
        [SerializeField] private float damageScaling = 1.05f;

        [Header("Events")]
        public UnityEvent<int> OnWaveStart;
        public UnityEvent<int> OnWaveComplete;
        public UnityEvent OnAllWavesComplete;

        private List<GameObject> activeEnemies = new List<GameObject>();
        private bool waveInProgress = false;
        private bool isBreakTime = false;
        private Transform player;
        private Camera mainCamera;

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            mainCamera = Camera.main;

            if (player == null)
            {
                Debug.LogError("❌ No se encontró al jugador. Asegúrate de que tenga el tag 'Player'");
            }

            // Verificar power-ups
            if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
            {
                Debug.LogWarning("⚠️ WaveManager: No hay Power-Up Prefabs asignados");
            }
            else
            {
                Debug.Log($"✅ WaveManager: {powerUpPrefabs.Length} tipos de power-ups configurados");
            }

            // Iniciar primera oleada después de un breve delay
            Invoke(nameof(StartNextWave), 2f);
        }

        private void Update()
        {
            // Limpiar lista de enemigos muertos
            activeEnemies.RemoveAll(enemy => enemy == null);

            // Verificar si la oleada terminó
            if (waveInProgress && activeEnemies.Count == 0)
            {
                CompleteWave();
            }
        }

        /// <summary>
        /// Inicia la siguiente oleada
        /// </summary>
        public void StartNextWave()
        {
            if (waveInProgress || isBreakTime) return;

            currentWave++;
            waveInProgress = true;

            Debug.Log($"🌊 === OLEADA {currentWave} ===");
            OnWaveStart?.Invoke(currentWave);

            // Generar oleada
            StartCoroutine(SpawnWave());
        }

        /// <summary>
        /// Genera los enemigos de la oleada actual
        /// </summary>
        private IEnumerator SpawnWave()
        {
            List<EnemySpawnData> wave = GenerateWaveData(currentWave);

            foreach (EnemySpawnData spawnData in wave)
            {
                for (int i = 0; i < spawnData.count; i++)
                {
                    SpawnEnemy(spawnData.enemyPrefab);
                    yield return new WaitForSeconds(spawnData.spawnDelay);
                }
            }
        }

        /// <summary>
        /// Genera los datos de spawn basados en el número de oleada
        /// </summary>
        private List<EnemySpawnData> GenerateWaveData(int wave)
        {
            List<EnemySpawnData> waveData = new List<EnemySpawnData>();

            // Oleadas 1-3: Solo gatos
            if (wave <= 3)
            {
                waveData.Add(new EnemySpawnData
                {
                    enemyPrefab = catPrefab,
                    count = 5 + (wave * 2),
                    spawnDelay = 0.5f
                });
            }
            // Oleadas 4-6: Gatos y algunos perros
            else if (wave <= 6)
            {
                waveData.Add(new EnemySpawnData
                {
                    enemyPrefab = catPrefab,
                    count = 8 + wave,
                    spawnDelay = 0.4f
                });
                waveData.Add(new EnemySpawnData
                {
                    enemyPrefab = dogPrefab,
                    count = 1 + (wave - 3),
                    spawnDelay = 1f
                });
            }
            // Oleadas 7+: Oleadas mixtas intensas
            else
            {
                waveData.Add(new EnemySpawnData
                {
                    enemyPrefab = catPrefab,
                    count = 10 + (wave * 2),
                    spawnDelay = 0.3f
                });
                waveData.Add(new EnemySpawnData
                {
                    enemyPrefab = dogPrefab,
                    count = 3 + wave,
                    spawnDelay = 0.8f
                });
            }

            return waveData;
        }

        /// <summary>
        /// Instancia un enemigo en una posición aleatoria
        /// </summary>
        private void SpawnEnemy(GameObject enemyPrefab)
        {
            if (enemyPrefab == null || player == null) return;

            // Encontrar posición de spawn dentro de los límites
            Vector2 spawnPosition = GetRandomSpawnPosition();

            // Instanciar enemigo
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            // Aplicar escalado de dificultad
            ApplyDifficultyScaling(enemy);

            // Agregar a lista de enemigos activos
            activeEnemies.Add(enemy);
        }

        /// <summary>
        /// Obtiene una posición aleatoria dentro de los límites del mapa
        /// </summary>
        private Vector2 GetRandomSpawnPosition()
        {
            Vector2 playerPos = player.position;
            Vector2 spawnPosition;
            int maxAttempts = 10;
            int attempts = 0;

            do
            {
                // Generar posición aleatoria en círculo alrededor del player
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(spawnDistanceFromPlayer, spawnRadius);
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                spawnPosition = playerPos + direction * distance;

                attempts++;

                // Si no usa bounds, retornar directamente
                if (!useMapBounds)
                {
                    return spawnPosition;
                }

                // Si está dentro de los límites, retornar
                if (IsWithinBounds(spawnPosition))
                {
                    return spawnPosition;
                }

            } while (attempts < maxAttempts);

            // Si después de 10 intentos no encontró posición válida,
            // forzar spawn en el borde del mapa más cercano al player
            return ClampToBounds(spawnPosition);
        }

        /// <summary>
        /// Verifica si una posición está dentro de los límites del mapa
        /// </summary>
        private bool IsWithinBounds(Vector2 position)
        {
            return position.x >= mapMinX && position.x <= mapMaxX &&
                   position.y >= mapMinY && position.y <= mapMaxY;
        }

        /// <summary>
        /// Ajusta una posición para que esté dentro de los límites
        /// </summary>
        private Vector2 ClampToBounds(Vector2 position)
        {
            return new Vector2(
                Mathf.Clamp(position.x, mapMinX, mapMaxX),
                Mathf.Clamp(position.y, mapMinY, mapMaxY)
            );
        }

        /// <summary>
        /// Aplica escalado de stats basado en la oleada actual
        /// </summary>
        private void ApplyDifficultyScaling(GameObject enemy)
        {
            Enemy.EnemyBase enemyScript = enemy.GetComponent<Enemy.EnemyBase>();
            if (enemyScript == null) return;

            float healthMultiplier = Mathf.Pow(healthScaling, currentWave - 1);
            float damageMultiplier = Mathf.Pow(damageScaling, currentWave - 1);

            enemyScript.SetMaxHealth(enemyScript.GetCurrentHealth() * healthMultiplier);
            enemyScript.SetDamage(enemyScript.GetCurrentHealth() * damageMultiplier);
        }

        /// <summary>
        /// Completa la oleada actual e inicia el descanso
        /// </summary>
        private void CompleteWave()
        {
            waveInProgress = false;
            isBreakTime = true;

            Debug.Log($"✅ ¡Oleada {currentWave} completada!");

            // Sonido de oleada completada (si existe)
           //AudioManager.PlayWaveCompleteSound();

            OnWaveComplete?.Invoke(currentWave);

            // Iniciar siguiente oleada después del break
            Invoke(nameof(EndBreakTime), timeBetweenWaves);
        }

        /// <summary>
        /// Termina el tiempo de descanso
        /// </summary>
        private void EndBreakTime()
        {
            isBreakTime = false;
            StartNextWave();
        }

        /// <summary>
        /// Llamado cuando un enemigo muere
        /// Puede spawnear power-ups
        /// </summary>
        public void OnEnemyDied(Enemy.EnemyBase enemy)
        {
            if (enemy == null) return;

            // Chance de spawn de power-up
            if (Random.value < powerUpSpawnChance)
            {
                SpawnRandomPowerUp(enemy.transform.position);
            }

            if (debugPowerUpSpawn)
            {
                Debug.Log($"⚔️ Enemigo eliminado. Quedan: {activeEnemies.Count - 1}");
            }
        }

        /// <summary>
        /// Spawna un power-up aleatorio en la posición especificada
        /// </summary>
        private void SpawnRandomPowerUp(Vector3 position)
        {
            if (powerUpPrefabs == null || powerUpPrefabs.Length == 0)
            {
                if (debugPowerUpSpawn)
                {
                    Debug.LogWarning("⚠️ No hay power-ups configurados para spawnear");
                }
                return;
            }

            // Offset aleatorio para que no aparezca exactamente donde murió el enemigo
            Vector2 offset = Random.insideUnitCircle * 1.5f;
            Vector3 spawnPos = position + (Vector3)offset;

            // Asegurar que esté dentro de los límites
            if (useMapBounds)
            {
                spawnPos = ClampToBounds(spawnPos);
            }

            // Seleccionar power-up aleatorio
            GameObject powerUpPrefab = powerUpPrefabs[Random.Range(0, powerUpPrefabs.Length)];

            if (powerUpPrefab != null)
            {
                Instantiate(powerUpPrefab, spawnPos, Quaternion.identity);

                if (debugPowerUpSpawn)
                {
                    Debug.Log($"🎁 Power-up spawneado: {powerUpPrefab.name} en {spawnPos}");
                }
            }
        }

        /// <summary>
        /// Fuerza el spawn de un power-up (útil para testing)
        /// </summary>
        public void ForceSpawnPowerUp(Vector3 position)
        {
            SpawnRandomPowerUp(position);
        }

        /// <summary>
        /// Dibuja los límites y radio de spawn en el editor
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (player != null)
            {
                // Radio de spawn
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(player.position, spawnRadius);

                // Distancia mínima
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(player.position, spawnDistanceFromPlayer);
            }

            // Límites del mapa
            if (useMapBounds)
            {
                Gizmos.color = Color.red;
                Vector3 topLeft = new Vector3(mapMinX, mapMaxY, 0);
                Vector3 topRight = new Vector3(mapMaxX, mapMaxY, 0);
                Vector3 bottomLeft = new Vector3(mapMinX, mapMinY, 0);
                Vector3 bottomRight = new Vector3(mapMaxX, mapMinY, 0);

                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);
                Gizmos.DrawLine(bottomLeft, topLeft);
            }
        }

        // Getters públicos
        public int GetCurrentWave() => currentWave;
        public bool IsBreakTime() => isBreakTime;
        public int GetActiveEnemyCount() => activeEnemies.Count;
    }
}