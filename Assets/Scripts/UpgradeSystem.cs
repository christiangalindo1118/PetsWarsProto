using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sistema de mejoras entre oleadas
/// Presenta opciones aleatorias al jugador para mejorar sus stats
/// </summary>
public class UpgradeSystem : MonoBehaviour
{
    [System.Serializable]
    public class Upgrade
    {
        public string name;
        public string description;
        public System.Action effect;
    }

    [Header("UI References")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private Button[] upgradeButtons;
    [SerializeField] private TextMeshProUGUI[] upgradeTexts;
    [SerializeField] private TextMeshProUGUI[] upgradeDescriptions;

    [Header("Game References")]
    [SerializeField] private Player.PlayerController playerController;
    [SerializeField] private Player.PlayerHealth playerHealth;
    [SerializeField] private Weapons.WeaponController weaponController;

    private WaveManager waveManager;
    private List<Upgrade> availableUpgrades = new List<Upgrade>();
    private List<Upgrade> currentUpgradeOptions = new List<Upgrade>();

    private void Start()
    {
        waveManager = FindFirstObjectByType<WaveManager>();

        // Suscribirse al evento de oleada completada
        if (waveManager != null)
        {
            waveManager.OnWaveComplete.AddListener(OnWaveComplete);
        }

        // Ocultar panel al inicio
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }

        // Inicializar mejoras disponibles
        InitializeUpgrades();

        // Configurar botones
        SetupUpgradeButtons();
    }

    /// <summary>
    /// Inicializa todas las mejoras disponibles
    /// </summary>
    private void InitializeUpgrades()
    {
        availableUpgrades.Clear();

        // Mejoras de movimiento
        availableUpgrades.Add(new Upgrade
        {
            name = "Speed+",
            description = "+20% movement speed",
            effect = () => {
                if (playerController != null)
                    playerController.SetMoveSpeed(playerController.GetComponent<Player.PlayerController>().GetType()
                        .GetField("moveSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .GetValue(playerController) is float speed ? speed * 1.2f : 6f);
            }
        });

        // Mejoras de salud
        availableUpgrades.Add(new Upgrade
        {
            name = "Max Health+",
            description = "+25 maximum health and complete healing",
            effect = () => {
                if (playerHealth != null)
                {
                    playerHealth.IncreaseMaxHealth(25f);
                    playerHealth.ResetHealth();
                }
            }
        });

        availableUpgrades.Add(new Upgrade
        {
            name = "Healing",
            description = "Restore 50% of your health",
            effect = () => {
                if (playerHealth != null)
                    playerHealth.Heal(playerHealth.GetMaxHealth() * 0.5f);
            }
        });

        // Mejoras de armas
        availableUpgrades.Add(new Upgrade
        {
            name = "Damage+",
            description = "+30% Weapon Damage",
            effect = () => {
                if (weaponController != null)
                    weaponController.UpgradeDamage(1.3f);
            }
        });

        availableUpgrades.Add(new Upgrade
        {
            name = "Cadence+",
            description = "+25% Shoot Speed",
            effect = () => {
                if (weaponController != null)
                    weaponController.UpgradeFireRate(1.25f);
            }
        });

        availableUpgrades.Add(new Upgrade
        {
            name = "New Gun",
            description = "Gun Additional (máx 6)",
            effect = () => {
                if (weaponController != null)
                    weaponController.AddWeapon();
            }
        });
    }

    /// <summary>
    /// Configura los listeners de los botones
    /// </summary>
    private void SetupUpgradeButtons()
    {
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            int index = i; // Captura para el closure
            if (upgradeButtons[i] != null)
            {
                upgradeButtons[i].onClick.AddListener(() => SelectUpgrade(index));
            }
        }
    }

    /// <summary>
    /// Llamado cuando se completa una oleada
    /// </summary>
    private void OnWaveComplete(int waveNumber)
    {
        ShowUpgradeOptions();
    }

    /// <summary>
    /// Muestra el panel de mejoras con opciones aleatorias
    /// </summary>
    private void ShowUpgradeOptions()
    {
        if (upgradePanel == null) return;

        // Pausar el juego
        Time.timeScale = 0;

        // Seleccionar 3 mejoras aleatorias
        currentUpgradeOptions = GetRandomUpgrades(3);

        // Actualizar UI
        for (int i = 0; i < upgradeButtons.Length && i < currentUpgradeOptions.Count; i++)
        {
            if (upgradeTexts[i] != null)
            {
                upgradeTexts[i].text = currentUpgradeOptions[i].name;
            }
            if (upgradeDescriptions[i] != null)
            {
                upgradeDescriptions[i].text = currentUpgradeOptions[i].description;
            }
            if (upgradeButtons[i] != null)
            {
                upgradeButtons[i].gameObject.SetActive(true);
            }
        }

        // Activar panel
        upgradePanel.SetActive(true);
    }

    /// <summary>
    /// Obtiene mejoras aleatorias sin repetir
    /// </summary>
    private List<Upgrade> GetRandomUpgrades(int count)
    {
        List<Upgrade> selected = new List<Upgrade>();
        List<Upgrade> pool = new List<Upgrade>(availableUpgrades);

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            selected.Add(pool[randomIndex]);
            pool.RemoveAt(randomIndex);
        }

        return selected;
    }

    /// <summary>
    /// Llamado cuando el jugador selecciona una mejora
    /// </summary>
    private void SelectUpgrade(int index)
    {
        if (index < 0 || index >= currentUpgradeOptions.Count) return;

        // Aplicar efecto
        currentUpgradeOptions[index].effect?.Invoke();

        Debug.Log($"Mejora seleccionada: {currentUpgradeOptions[index].name}");

        // Ocultar panel
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
        }

        // Reanudar juego
        Time.timeScale = 1;

        // Limpiar opciones
        currentUpgradeOptions.Clear();
    }
}