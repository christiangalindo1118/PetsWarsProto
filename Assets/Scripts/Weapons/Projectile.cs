using UnityEngine;
using PetsWars.VFX;
using PetsWars;

namespace Weapons
{
    /// <summary>
    /// Proyectil básico que se mueve en una dirección y causa daño
    /// Se destruye al impactar o después de un tiempo
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Stats")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifetime = 3f;

        [Header("Visual")]
        [SerializeField] private Color projectileColor = Color.yellow;

        private Rigidbody2D rb;
        private float lifeTimer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            
            // Configurar el sprite renderer si existe
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = projectileColor;
            }
        }

        private void Start()
        {
            lifeTimer = lifetime;
        }

        private void Update()
        {
            // Destruir después del tiempo de vida
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                DestroyProjectile();
            }
        }

        /// <summary>
        /// Inicializa el proyectil con dirección y stats opcionales
        /// </summary>
        public void Initialize(Vector2 direction, float customDamage = -1f, float customSpeed = -1f)
        {
            // Usar stats personalizados si se proporcionan
            if (customDamage > 0) damage = customDamage;
            if (customSpeed > 0) speed = customSpeed;

            // Establecer velocidad
            rb.linearVelocity = direction.normalized * speed;

            // Rotar el proyectil en la dirección de movimiento
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            
            AudioManager.PlayShootSound();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Verificar si impactó un enemigo
            if (other.CompareTag("Enemy"))
            {
                // Intentar causar daño
                IDamageable damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                    
                    AudioManager.PlayHitSound();
                    
                    SimpleVFX.CreateHitEffect(transform.position, Color.yellow);
                }

                DestroyProjectile();
            }
        }

        /// <summary>
        /// Destruye el proyectil de forma segura
        /// </summary>
        private void DestroyProjectile()
        {
            // Aquí podrías instanciar un efecto de partículas
            Destroy(gameObject);
        }

        /// <summary>
        /// Permite modificar el daño externamente
        /// </summary>
        public void SetDamage(float newDamage)
        {
            damage = newDamage;
        }

        /// <summary>
        /// Permite modificar la velocidad externamente
        /// </summary>
        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
            if (rb != null && rb.linearVelocity.magnitude > 0)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * speed;
            }
        }
    }
}
