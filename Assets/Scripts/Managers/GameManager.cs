using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace PetsWars
{
    /// <summary>
    /// Gestor principal del juego
    /// Controla el estado general, pausas y reinicio
    /// Compatible con New Input System
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Game State")]
        [SerializeField] private bool isPaused = false;
        [SerializeField] private bool isGameOver = false;

        [Header("UI References")]
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject gameOverMenu;

        [Header("Input Actions")]
        [SerializeField] private InputActionAsset inputActions;

        private InputAction pauseAction;
        private InputAction restartAction;
        private static GameManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Inicializar Input Actions
            SetupInputActions();
        }

        private void OnEnable()
        {
            // Activar acciones
            if (pauseAction != null) pauseAction.Enable();
            if (restartAction != null) restartAction.Enable();
        }

        private void OnDisable()
        {
            // Desactivar acciones
            if (pauseAction != null) pauseAction.Disable();
            if (restartAction != null) restartAction.Disable();
        }

        private void Start()
        {
            // Ocultar menús al inicio
            if (pauseMenu != null) pauseMenu.SetActive(false);
            if (gameOverMenu != null) gameOverMenu.SetActive(false);

            // Suscribirse al evento de muerte del jugador
            Player.PlayerHealth playerHealth = FindFirstObjectByType<Player.PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnPlayerDied.AddListener(OnPlayerDied);
            }

            // Asegurar que el tiempo esté corriendo
            Time.timeScale = 1f;

            Debug.Log("✅ GameManager inicializado");
        }

        /// <summary>
        /// Configura las acciones de input
        /// </summary>
        private void SetupInputActions()
        {
            // Si tienes PlayerInputActions, úsalo
            // Si no, creamos acciones directamente
            if (inputActions != null)
            {
                // Usar el InputActionAsset si está asignado
                var gameplayMap = inputActions.FindActionMap("Player");
                if (gameplayMap != null)
                {
                    pauseAction = gameplayMap.FindAction("Pause");
                    restartAction = gameplayMap.FindAction("Restart");
                }
            }

            // Si no hay InputActionAsset, crear acciones directamente
            if (pauseAction == null)
            {
                pauseAction = new InputAction("Pause", binding: "<Keyboard>/escape");
                pauseAction.performed += ctx => TogglePause();
            }
            else
            {
                pauseAction.performed += ctx => TogglePause();
            }

            if (restartAction == null)
            {
                restartAction = new InputAction("Restart", binding: "<Keyboard>/r");
                restartAction.performed += ctx => { if (isGameOver) RestartGame(); };
            }
            else
            {
                restartAction.performed += ctx => { if (isGameOver) RestartGame(); };
            }
        }

        /// <summary>
        /// Alterna entre pausado y no pausado
        /// </summary>
        public void TogglePause()
        {
            if (isGameOver) return;

            isPaused = !isPaused;

            if (isPaused)
            {
                Time.timeScale = 0f;
                if (pauseMenu != null) pauseMenu.SetActive(true);
                Debug.Log("⏸️ Juego pausado");
            }
            else
            {
                Time.timeScale = 1f;
                if (pauseMenu != null) pauseMenu.SetActive(false);
                Debug.Log("▶️ Juego reanudado");
            }
        }

        /// <summary>
        /// Llamado cuando el jugador muere
        /// </summary>
        private void OnPlayerDied()
        {
            isGameOver = true;

            if (gameOverMenu != null)
            {
                gameOverMenu.SetActive(true);
            }

            Debug.Log("💀 GAME OVER - Presiona R para reiniciar");
        }

        /// <summary>
        /// Reinicia el juego
        /// </summary>
        public void RestartGame()
        {
            Debug.Log("🔄 Reiniciando juego...");
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        /// <summary>
        /// Sale del juego
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("👋 Saliendo del juego...");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Reanuda el juego desde pausa
        /// </summary>
        public void ResumeGame()
        {
            if (isPaused)
            {
                TogglePause();
            }
        }

        private void OnDestroy()
        {
            // Limpiar suscripciones
            if (pauseAction != null)
            {
                pauseAction.performed -= ctx => TogglePause();
                pauseAction.Disable();
                pauseAction.Dispose();
            }

            if (restartAction != null)
            {
                restartAction.performed -= ctx => { if (isGameOver) RestartGame(); };
                restartAction.Disable();
                restartAction.Dispose();
            }
        }

        // Getters
        public static GameManager Instance => instance;
        public bool IsPaused => isPaused;
        public bool IsGameOver => isGameOver;
    }
}

