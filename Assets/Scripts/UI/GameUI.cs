using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PetsWars;

namespace UI
{
    /// <summary>
    /// Controla la interfaz principal del juego (HUD)
    /// Muestra salud, oleada actual, experiencia y nivel
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField] private RectTransform healthBarFill;
        [SerializeField] private Image healthBarImage;
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Wave Info")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Break UI")]
        [SerializeField] private GameObject breakPanel;
        [SerializeField] private TextMeshProUGUI breakTimerText;

        [Header("Experience Bar")]
        [SerializeField] private Image xpBarFill;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI xpText;

        [Header("Health Bar Colors")]
        [SerializeField] private Color healthyColor = Color.green;
        [SerializeField] private Color damagedColor = Color.yellow;
        [SerializeField] private Color criticalColor = Color.red;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = false;

        private Player.PlayerHealth playerHealth;
        private WaveManager waveManager;
        private ExperienceSystem expSystem;

        private float maxHealthBarWidth;
        private float breakTimer;

        private void Start()
        {
            // Encontrar referencias
            playerHealth = FindFirstObjectByType<Player.PlayerHealth>();
            waveManager = FindFirstObjectByType<WaveManager>();
            expSystem = ExperienceSystem.Instance;

            // Configurar barra de salud
            if (healthBarFill != null)
            {
                maxHealthBarWidth = healthBarFill.sizeDelta.x;
            }

            // Suscribirse a eventos de salud
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
                UpdateHealthBar(1f); // Inicializar
            }
            else
            {
                Debug.LogWarning("⚠️ GameUI: PlayerHealth no encontrado");
            }

            // Suscribirse a eventos de oleadas
            if (waveManager != null)
            {
                waveManager.OnWaveStart.AddListener(OnWaveStart);
                waveManager.OnWaveComplete.AddListener(OnWaveComplete);
            }
            else
            {
                Debug.LogWarning("⚠️ GameUI: WaveManager no encontrado");
            }

            // Suscribirse a eventos de experiencia
            if (expSystem != null)
            {
                expSystem.OnXPChanged.AddListener(UpdateXPBar);
                expSystem.OnLevelUp.AddListener(OnLevelUp);
                
                // Inicializar barra de XP
                UpdateXPBar(expSystem.GetCurrentXP(), expSystem.GetXPToNextLevel());

                if (showDebugLogs)
                {
                    Debug.Log("✅ GameUI: Suscrito a ExperienceSystem");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ GameUI: ExperienceSystem no encontrado");
            }

            // Ocultar panel de descanso
            if (breakPanel != null)
            {
                breakPanel.SetActive(false);
            }

            if (showDebugLogs)
            {
                Debug.Log("✅ GameUI inicializado correctamente");
            }
        }

        private void Update()
        {
            UpdateWaveDisplay();
            UpdateBreakTimer();
        }

        /// <summary>
        /// Actualiza la barra y texto de salud (Unity 6 compatible)
        /// </summary>
        public void UpdateHealthBar(float healthPercentage)
        {
            if (healthBarFill != null)
            {
                healthBarFill.sizeDelta = new Vector2(
                    maxHealthBarWidth * healthPercentage,
                    healthBarFill.sizeDelta.y
                );

                if (healthBarImage != null)
                {
                    if (healthPercentage > 0.6f)
                        healthBarImage.color = healthyColor;
                    else if (healthPercentage > 0.3f)
                        healthBarImage.color = damagedColor;
                    else
                        healthBarImage.color = criticalColor;
                }
            }

            if (healthText != null && playerHealth != null)
            {
                float current = playerHealth.GetCurrentHealth();
                float max = playerHealth.GetMaxHealth();
                healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
            }
        }

        /// <summary>
        /// Actualiza la barra de experiencia
        /// </summary>
        private void UpdateXPBar(float currentXP, float requiredXP)
        {
            if (xpBarFill != null)
            {
                float fillAmount = currentXP / requiredXP;
                xpBarFill.fillAmount = fillAmount;

                if (showDebugLogs)
                {
                    Debug.Log($"📊 XP Bar actualizada: {fillAmount * 100:F1}%");
                }
            }

            if (levelText != null && expSystem != null)
            {
                levelText.text = $"Nivel {expSystem.GetCurrentLevel()}";
            }

            if (xpText != null)
            {
                xpText.text = $"{Mathf.FloorToInt(currentXP)} / {Mathf.FloorToInt(requiredXP)} XP";
            }
        }

        /// <summary>
        /// Llamado cuando el jugador sube de nivel
        /// </summary>
        private void OnLevelUp(int newLevel)
        {
            Debug.Log($"🎉 UI: ¡Nivel {newLevel} alcanzado!");

            // Animación simple del texto de nivel
            if (levelText != null)
            {
                StartCoroutine(LevelUpAnimation());
            }
        }

        /// <summary>
        /// Animación simple cuando subes de nivel
        /// </summary>
        private System.Collections.IEnumerator LevelUpAnimation()
        {
            if (levelText == null) yield break;

            Vector3 originalScale = levelText.transform.localScale;
            Color originalColor = levelText.color;

            // Agrandar y hacer dorado
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Escala
                float scale = Mathf.Lerp(1.5f, 1f, t);
                levelText.transform.localScale = originalScale * scale;

                // Color (dorado a blanco)
                levelText.color = Color.Lerp(Color.yellow, originalColor, t);

                yield return null;
            }

            levelText.transform.localScale = originalScale;
            levelText.color = originalColor;
        }

        /// <summary>
        /// Actualiza el texto de oleadas
        /// </summary>
        private void UpdateWaveDisplay()
        {
            if (waveText != null && waveManager != null)
            {
                waveText.text = $"Wave {waveManager.GetCurrentWave()}";
            }
        }

        /// <summary>
        /// Maneja el contador del descanso entre oleadas
        /// </summary>
        private void UpdateBreakTimer()
        {
            if (waveManager == null) return;

            if (waveManager.IsBreakTime())
            {
                if (breakPanel != null && !breakPanel.activeSelf)
                {
                    breakPanel.SetActive(true);
                    breakTimer = 10f;
                }

                breakTimer -= Time.deltaTime;

                if (breakTimerText != null)
                {
                    breakTimerText.text = $"Next Wave in: {Mathf.CeilToInt(breakTimer)}s";
                }
            }
            else
            {
                if (breakPanel != null && breakPanel.activeSelf)
                {
                    breakPanel.SetActive(false);
                }
            }
        }

        private void OnWaveStart(int waveNumber)
        {
            if (showDebugLogs)
            {
                Debug.Log($"🌊 UI: Oleada {waveNumber} iniciada");
            }
        }

        private void OnWaveComplete(int waveNumber)
        {
            if (showDebugLogs)
            {
                Debug.Log($"✅ UI: Oleada {waveNumber} completada");
            }
        }

        private void OnDestroy()
        {
            // Desuscribirse de eventos
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged.RemoveListener(UpdateHealthBar);
            }

            if (waveManager != null)
            {
                waveManager.OnWaveStart.RemoveListener(OnWaveStart);
                waveManager.OnWaveComplete.RemoveListener(OnWaveComplete);
            }

            if (expSystem != null)
            {
                expSystem.OnXPChanged.RemoveListener(UpdateXPBar);
                expSystem.OnLevelUp.RemoveListener(OnLevelUp);
            }
        }
    }
}

