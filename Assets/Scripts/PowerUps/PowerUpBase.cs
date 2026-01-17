using UnityEngine;

namespace PetsWars.PowerUps
{
    /// <summary>
    /// Clase base para todos los power-ups
    /// Los power-ups aparecen en el suelo y dan beneficios al jugador
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class PowerUpBase : MonoBehaviour
    {
        [Header("Power-Up Settings")]
        [SerializeField] protected float lifetime = 10f;
        [SerializeField] protected bool autoDestroy = true;

        [Header("Visual")]
        [SerializeField] protected Color powerUpColor = Color.yellow;
        [SerializeField] protected float floatAmplitude = 0.2f;
        [SerializeField] protected float floatSpeed = 2f;
        [SerializeField] protected float rotationSpeed = 90f;

        protected SpriteRenderer spriteRenderer;
        protected Vector3 startPosition;
        protected float lifeTimer;
        protected bool isCollected = false;

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = powerUpColor;
            }

            // Configurar collider como trigger
            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
                collider.radius = 0.5f;
            }
        }

        protected virtual void Start()
        {
            startPosition = transform.position;
            lifeTimer = lifetime;
        }

        protected virtual void Update()
        {
            if (isCollected) return;

            // Animación de flotación
            FloatAnimation();

            // Rotación
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            // Auto-destrucción
            if (autoDestroy)
            {
                lifeTimer -= Time.deltaTime;
                if (lifeTimer <= 0)
                {
                    DestroyPowerUp();
                }

                // Parpadeo cuando está por expirar
                if (lifeTimer < 3f && spriteRenderer != null)
                {
                    float alpha = Mathf.PingPong(Time.time * 5f, 1f);
                    Color color = spriteRenderer.color;
                    color.a = Mathf.Lerp(0.3f, 1f, alpha);
                    spriteRenderer.color = color;
                }
            }
        }

        /// <summary>
        /// Animación de flotación suave
        /// </summary>
        protected virtual void FloatAnimation()
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            if (other.CompareTag("Player"))
            {
                CollectPowerUp(other.gameObject);
            }
        }

        /// <summary>
        /// Método llamado cuando el jugador recoge el power-up
        /// Sobrescribir en clases derivadas
        /// </summary>
        protected virtual void CollectPowerUp(GameObject player)
        {
            isCollected = true;
            Debug.Log($"💎 Power-up recogido: {GetType().Name}");

            // Efecto visual
            VFX.SimpleVFX.CreateHitEffect(transform.position, powerUpColor);

            DestroyPowerUp();
        }

        /// <summary>
        /// Destruye el power-up
        /// </summary>
        protected virtual void DestroyPowerUp()
        {
            Destroy(gameObject);
        }
    }
}