using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    /// <summary>
    /// Controla el movimiento del jugador usando el New Input System
    /// Soporta WASD, flechas y gamepad automáticamente
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float acceleration = 50f;

        [Header("Rotation Settings")]
        [SerializeField] private bool rotateTowardsMouse = true;
        [SerializeField] private float rotationSpeed = 720f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;

        private Rigidbody2D rb;
        private Camera mainCamera;
        private Vector2 moveInput;
        private Vector2 lookPosition;
        private PlayerInputActions inputActions;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            
            // Configurar Rigidbody2D
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.linearDamping = 5f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Inicializar Input Actions
            inputActions = new PlayerInputActions();

            Debug.Log("✅ PlayerController (New Input System) inicializado");
        }

        private void Start()
        {
            mainCamera = Camera.main;
            
            if (mainCamera == null)
            {
                Debug.LogError("❌ No se encontró la Main Camera");
            }
        }

        private void OnEnable()
        {
            // Activar las acciones
            inputActions.Player.Enable();

            // Suscribirse a eventos de movimiento
            inputActions.Player.Movement.performed += OnMovement;
            inputActions.Player.Movement.canceled += OnMovement;

            // Suscribirse a eventos del mouse
            inputActions.Player.Look.performed += OnLook;
        }

        private void OnDisable()
        {
            // Desactivar las acciones
            inputActions.Player.Disable();

            // Desuscribirse de eventos
            inputActions.Player.Movement.performed -= OnMovement;
            inputActions.Player.Movement.canceled -= OnMovement;
            inputActions.Player.Look.performed -= OnLook;
        }

        private void Update()
        {
            if (rotateTowardsMouse && mainCamera != null)
            {
                RotateTowardsMouse();
            }

            // Debug info
            if (showDebugInfo && moveInput.magnitude > 0.1f)
            {
                Debug.Log($"🎮 Input: {moveInput} | Velocidad: {rb.linearVelocity.magnitude:F2}");
            }
        }

        private void FixedUpdate()
        {
            HandleMovement();
        }

        /// <summary>
        /// Callback cuando se recibe input de movimiento
        /// </summary>
        private void OnMovement(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// Callback cuando se recibe input del mouse/look
        /// </summary>
        private void OnLook(InputAction.CallbackContext context)
        {
            lookPosition = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// Mueve el jugador usando física con aceleración suave
        /// </summary>
        private void HandleMovement()
        {
            if (rb == null) return;

            Vector2 targetVelocity = moveInput * moveSpeed;
            Vector2 velocityDifference = targetVelocity - rb.linearVelocity;
            Vector2 movement = velocityDifference * acceleration * Time.fixedDeltaTime;
            
            rb.AddForce(movement, ForceMode2D.Force);
        }

        /// <summary>
        /// Rota el jugador hacia la posición del mouse
        /// </summary>
        private void RotateTowardsMouse()
        {
            if (mainCamera == null) return;

            // Convertir posición de pantalla a mundo
            Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(lookPosition);
            Vector2 direction = (mouseWorldPosition - (Vector2)transform.position).normalized;
            
            if (direction.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                float currentAngle = transform.eulerAngles.z;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
                
                transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
            }
        }

        /// <summary>
        /// Dibuja debug info en la escena
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showDebugInfo || !Application.isPlaying) return;

            // Dirección de movimiento (verde)
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(moveInput * 2f));

            // Dirección hacia el mouse (rojo)
            if (mainCamera != null)
            {
                Gizmos.color = Color.red;
                Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(lookPosition);
                Vector2 mouseDir = (mouseWorldPos - (Vector2)transform.position).normalized;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)(mouseDir * 1.5f));
            }

            // Círculo alrededor del player
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }

        // Getters públicos para otros sistemas
        public Vector2 GetMoveDirection() => moveInput;
        
        public Vector2 GetAimDirection()
        {
            if (mainCamera == null) return Vector2.up;
            Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(lookPosition);
            return (mouseWorldPos - (Vector2)transform.position).normalized;
        }

        public void SetMoveSpeed(float newSpeed)
        {
            moveSpeed = newSpeed;
        }

        public float GetCurrentSpeed() => moveSpeed;
    }
}