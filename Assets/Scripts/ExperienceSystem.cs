using UnityEngine;
using UnityEngine.Events;

namespace PetsWars
{
    /// <summary>
    /// Sistema de experiencia y niveles del jugador
    /// Maneja XP, niveles y stats permanentes
    /// </summary>
    public class ExperienceSystem : MonoBehaviour
    {
        [Header("Level Settings")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private float currentXP = 0f;
        [SerializeField] private float xpToNextLevel = 100f;
        [SerializeField] private float xpScalingPerLevel = 1.5f;

        [Header("XP Rewards")]
        [SerializeField] private float xpPerCatKill = 10f;
        [SerializeField] private float xpPerDogKill = 25f;
        [SerializeField] private float xpPerBossKill = 200f;

        [Header("Level Up Bonuses")]
        [SerializeField] private float healthBonusPerLevel = 10f;
        [SerializeField] private float damageBonusPerLevel = 5f;
        [SerializeField] private float speedBonusPerLevel = 0.1f;

        [Header("Events")]
        public UnityEvent<int> OnLevelUp;
        public UnityEvent<float> OnXPGained;
        public UnityEvent<float, float> OnXPChanged; // current, required

        [Header("References")]
        [SerializeField] private Player.PlayerHealth playerHealth;
        [SerializeField] private Player.PlayerController playerController;
        [SerializeField] private Weapons.WeaponController weaponController;

        private static ExperienceSystem instance;
        private int totalKills = 0;

        public static ExperienceSystem Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Auto-encontrar referencias si no están asignadas
            if (playerHealth == null)
                playerHealth = FindFirstObjectByType<Player.PlayerHealth>();
            if (playerController == null)
                playerController = FindFirstObjectByType<Player.PlayerController>();
            if (weaponController == null)
                weaponController = FindFirstObjectByType<Weapons.WeaponController>();
        }

        private void Start()
        {
            Debug.Log($"✅ ExperienceSystem inicializado. Nivel: {currentLevel}");
            OnXPChanged?.Invoke(currentXP, xpToNextLevel);
        }

        /// <summary>
        /// Otorga XP al jugador
        /// </summary>
        public void GainXP(float amount)
        {
            currentXP += amount;
            OnXPGained?.Invoke(amount);
            OnXPChanged?.Invoke(currentXP, xpToNextLevel);

            Debug.Log($"⭐ +{amount} XP | Total: {currentXP}/{xpToNextLevel}");

            // Verificar level up
            while (currentXP >= xpToNextLevel)
            {
                LevelUp();
            }
        }

        /// <summary>
        /// Sube de nivel al jugador
        /// </summary>
        private void LevelUp()
        {
            currentXP -= xpToNextLevel;
            currentLevel++;

            // Calcular XP necesaria para siguiente nivel
            xpToNextLevel = Mathf.Floor(xpToNextLevel * xpScalingPerLevel);

            Debug.Log($"🎉 ¡LEVEL UP! Nivel {currentLevel}");

            // Aplicar bonificaciones
            ApplyLevelUpBonuses();

            // Notificar evento
            OnLevelUp?.Invoke(currentLevel);
            OnXPChanged?.Invoke(currentXP, xpToNextLevel);

            // Efecto visual
            VFX.SimpleVFX.ScreenFlash(new Color(1f, 1f, 0f, 0.5f), 0.5f);
        }

        /// <summary>
        /// Aplica las bonificaciones de subir de nivel
        /// </summary>
        private void ApplyLevelUpBonuses()
        {
            // Bonificación de salud
            if (playerHealth != null)
            {
                playerHealth.IncreaseMaxHealth(healthBonusPerLevel);
                playerHealth.ResetHealth(); // Curación completa al subir de nivel
            }

            // Bonificación de daño
            if (weaponController != null)
            {
                float damageMultiplier = 1f + (damageBonusPerLevel / 100f);
                weaponController.UpgradeDamage(damageMultiplier);
            }

            // Bonificación de velocidad
            if (playerController != null)
            {
                float currentSpeed = playerController.GetCurrentSpeed();
                playerController.SetMoveSpeed(currentSpeed + speedBonusPerLevel);
            }

            Debug.Log($"📈 Bonificaciones aplicadas: +{healthBonusPerLevel} HP, +{damageBonusPerLevel}% DMG, +{speedBonusPerLevel} SPD");
        }

        /// <summary>
        /// Otorga XP por matar un enemigo
        /// </summary>
        public void OnEnemyKilled(string enemyType)
        {
            totalKills++;

            float xpAmount = 0f;

            switch (enemyType.ToLower())
            {
                case "cat":
                    xpAmount = xpPerCatKill;
                    break;
                case "dog":
                    xpAmount = xpPerDogKill;
                    break;
                case "boss":
                    xpAmount = xpPerBossKill;
                    break;
                default:
                    xpAmount = 10f; // XP por defecto
                    break;
            }

            GainXP(xpAmount);
        }

        /// <summary>
        /// Resetea el progreso (para nuevo juego)
        /// </summary>
        public void ResetProgress()
        {
            currentLevel = 1;
            currentXP = 0f;
            xpToNextLevel = 100f;
            totalKills = 0;
            OnXPChanged?.Invoke(currentXP, xpToNextLevel);
        }

        // Getters públicos
        public int GetCurrentLevel() => currentLevel;
        public float GetCurrentXP() => currentXP;
        public float GetXPToNextLevel() => xpToNextLevel;
        public float GetXPPercentage() => currentXP / xpToNextLevel;
        public int GetTotalKills() => totalKills;
    }
}
