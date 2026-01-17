using UnityEngine;
using UnityEngine.Events;
using PetsWars.VFX;
using PetsWars;
using PetsWars.PowerUps;

namespace Player
{
    /// <summary>
    /// Maneja la salud del jugador y la lógica de muerte
    /// Implementa IDamageable para recibir daño
    /// Integrado con Sistema de Escudo, VFX y Audio
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("Invincibility")]
        [SerializeField] private float invincibilityDuration = 0.5f;
        [SerializeField] private float invincibilityFlashSpeed = 0.1f;

        [Header("Visual Feedback")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color damageColor = Color.red;

        [Header("Events")]
        public UnityEvent<float> OnHealthChanged;
        public UnityEvent OnPlayerDied;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        private bool isInvincible = false;
        private float invincibilityTimer;
        private SpriteRenderer spriteRenderer;
        private bool isAlive = true;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            currentHealth = maxHealth;

            if (showDebugLogs)
            {
                Debug.Log($"✅ PlayerHealth inicializado. HP: {currentHealth}/{maxHealth}");
            }
        }

        private void Update()
        {
            if (isInvincible)
            {
                HandleInvincibility();
            }
        }

        /// <summary>
        /// Implementación de IDamageable: recibe daño
        /// </summary>
        public void TakeDamage(float damageAmount)
        {
            if (!isAlive)
            {
                if (showDebugLogs) Debug.Log("❌ Player ya está muerto");
                return;
            }

            if (isInvincible)
            {
                if (showDebugLogs) Debug.Log("🛡️ Player invencible, daño bloqueado");
                return;
            }

            // 🛡️ VERIFICAR ESCUDO DE POWER-UP
            ShieldEffect shield = GetComponent<ShieldEffect>();
            if (shield != null && shield.IsActive())
            {
                Debug.Log("🛡️ ¡Daño bloqueado por escudo de power-up!");
                return; // No recibir daño
            }

            currentHealth -= damageAmount;
            currentHealth = Mathf.Max(currentHealth, 0);

            if (showDebugLogs)
            {
                Debug.Log($"💔 Player recibió {damageAmount} daño. Salud: {currentHealth}/{maxHealth}");
            }

            // 🔊 SONIDO DE DAÑO
            AudioManager.PlayPlayerHitSound();

            // 🎨 FLASH ROJO EN PANTALLA
            SimpleVFX.ScreenFlash(new Color(1f, 0f, 0f, 0.3f), 0.2f);

            // 📷 CAMERA SHAKE (si existe)
            CameraFollow cam = Camera.main?.GetComponent<CameraFollow>();
            if (cam != null)
            {
               //cam.Shake(0.15f, 0.2f);
            }

            // Notificar cambio de salud (para UI)
            OnHealthChanged?.Invoke(currentHealth / maxHealth);

            // Activar invencibilidad temporal
            ActivateInvincibility();

            // Verificar muerte
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Activa la invencibilidad temporal después de recibir daño
        /// </summary>
        private void ActivateInvincibility()
        {
            isInvincible = true;
            invincibilityTimer = invincibilityDuration;

            if (showDebugLogs)
            {
                Debug.Log($"🛡️ Invencibilidad activada por {invincibilityDuration}s");
            }
        }

        /// <summary>
        /// Maneja el estado de invencibilidad y el efecto de parpadeo
        /// </summary>
        private void HandleInvincibility()
        {
            invincibilityTimer -= Time.deltaTime;

            // Parpadeo visual
            if (spriteRenderer != null)
            {
                float alpha = Mathf.PingPong(Time.time * (1f / invincibilityFlashSpeed), 1f);
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(0.3f, 1f, alpha);
                spriteRenderer.color = color;
            }

            // Terminar invencibilidad
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                if (spriteRenderer != null)
                {
                    Color color = spriteRenderer.color;
                    color.a = 1f;
                    spriteRenderer.color = color;
                }

                if (showDebugLogs)
                {
                    Debug.Log("🛡️ Invencibilidad terminada");
                }
            }
        }

        /// <summary>
        /// Maneja la muerte del jugador
        /// </summary>
        private void Die()
        {
            if (!isAlive) return;

            isAlive = false;
            Debug.Log("💀 ¡Jugador muerto! Game Over");

            // Notificar evento de muerte
            OnPlayerDied?.Invoke();

            // Detener el tiempo (para menú de game over)
            Time.timeScale = 0;
        }

        /// <summary>
        /// Cura al jugador
        /// </summary>
        public void Heal(float amount)
        {
            if (!isAlive) return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            OnHealthChanged?.Invoke(currentHealth / maxHealth);

            if (showDebugLogs)
            {
                Debug.Log($"💚 Player curado {amount}. Salud: {currentHealth}/{maxHealth}");
            }
        }

        /// <summary>
        /// Aumenta la salud máxima (usado en level ups)
        /// </summary>
        public void IncreaseMaxHealth(float amount)
        {
            maxHealth += amount;
            currentHealth += amount;

            OnHealthChanged?.Invoke(currentHealth / maxHealth);

            if (showDebugLogs)
            {
                Debug.Log($"⬆️ Salud máxima aumentada +{amount}. Nueva máxima: {maxHealth}");
            }
        }

        /// <summary>
        /// Reinicia la salud al máximo (usado en level ups)
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(1f);

            if (showDebugLogs)
            {
                Debug.Log($"🔄 Salud reiniciada a {maxHealth}");
            }
        }

        // ============================================
        // IMPLEMENTACIÓN DE IDamageable
        // ============================================

        /// <summary>
        /// Verifica si el jugador está vivo
        /// </summary>
        public bool IsAlive() => isAlive;

        /// <summary>
        /// Obtiene la salud actual del jugador
        /// </summary>
        public float GetCurrentHealth() => currentHealth;

        // ============================================
        // GETTERS ADICIONALES
        // ============================================

        public float GetMaxHealth() => maxHealth;
        public float GetHealthPercentage() => currentHealth / maxHealth;
        public bool IsInvincible() => isInvincible;
    }
}