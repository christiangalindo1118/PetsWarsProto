using UnityEngine;

namespace PetsWars.PowerUps
{
    /// <summary>
    /// Power-up que restaura salud al jugador
    /// </summary>
    public class HealthPowerUp : PowerUpBase
    {
        [Header("Health Power-Up")]
        [SerializeField] private float healAmount = 30f;
        [SerializeField] private bool healPercentage = false;
        [SerializeField] private float healPercentageAmount = 0.5f; // 50%

        protected override void Awake()
        {
            powerUpColor = new Color(0f, 1f, 0.3f); // Verde brillante
            base.Awake();
        }

        protected override void CollectPowerUp(GameObject player)
        {
            Player.PlayerHealth playerHealth = player.GetComponent<Player.PlayerHealth>();
            
            if (playerHealth != null)
            {
                if (healPercentage)
                {
                    float maxHealth = playerHealth.GetMaxHealth();
                    float healAmount = maxHealth * healPercentageAmount;
                    playerHealth.Heal(healAmount);
                    Debug.Log($"💚 +{healAmount} HP ({healPercentageAmount * 100}%)");
                }
                else
                {
                    playerHealth.Heal(healAmount);
                    Debug.Log($"💚 +{healAmount} HP");
                }

                // Sonido (si existe)
                // AudioManager.PlayHealSound();
            }

            base.CollectPowerUp(player);
        }
    }
}