using Managers;
using UnityEngine;
using PetsWars.VFX;
using PetsWars;

namespace Enemy
{
    /// <summary>
    /// Clase base para todos los enemigos
    /// Maneja persecución del jugador, salud y daño
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyBase : MonoBehaviour, IDamageable
    {
        [Header("Stats")]
        [SerializeField] protected float maxHealth = 50f;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected float damage = 10f;

        [Header("Behavior")]
        [SerializeField] protected float attackRange = 1f;
        [SerializeField] protected float attackCooldown = 1f;

        [Header("Visual Feedback")]
        [SerializeField] protected Color normalColor = Color.white;
        [SerializeField] protected Color damageColor = Color.red;
        [SerializeField] protected float damageFlashDuration = 0.1f;

        protected Transform player;
        protected Rigidbody2D rb;
        protected SpriteRenderer spriteRenderer;
        protected float lastAttackTime;
        protected bool isAlive = true;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            currentHealth = maxHealth;
        }

        protected virtual void Start()
        {
            // Encontrar al jugador
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("No se encontró GameObject con tag 'Player'");
            }

            // Asegurar color normal
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
        }

        protected virtual void Update()
        {
            if (!isAlive || player == null) return;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // Atacar si está en rango
            if (distanceToPlayer <= attackRange)
            {
                AttemptAttack();
            }
        }

        protected virtual void FixedUpdate()
        {
            if (!isAlive || player == null) return;

            MoveTowardsPlayer();
        }

        /// <summary>
        /// Mueve al enemigo hacia el jugador
        /// </summary>
        protected virtual void MoveTowardsPlayer()
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= attackRange * 0.9f)
            {
                rb.linearVelocity = Vector2.zero;
                return;
            }

            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }

        /// <summary>
        /// Intenta atacar al jugador si el cooldown ha terminado
        /// </summary>
        protected virtual void AttemptAttack()
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }

        /// <summary>
        /// Ejecuta el ataque al jugador
        /// </summary>
        protected virtual void Attack()
        {
            IDamageable playerDamageable = player.GetComponent<IDamageable>();
            if (playerDamageable != null)
            {
                playerDamageable.TakeDamage(damage);
            }
        }

        /// <summary>
        /// Implementación de IDamageable: recibe daño
        /// </summary>
        public virtual void TakeDamage(float damageAmount)
        {
            if (!isAlive) return;

            currentHealth -= damageAmount;

            // Feedback visual de daño
            if (spriteRenderer != null)
            {
                StartCoroutine(DamageFlash());
            }

            // Verificar muerte
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Flash visual al recibir daño
        /// </summary>
        protected System.Collections.IEnumerator DamageFlash()
        {
            spriteRenderer.color = damageColor;
            yield return new WaitForSeconds(damageFlashDuration);
            spriteRenderer.color = normalColor;
        }

        /// <summary>
        /// Maneja la muerte del enemigo
        /// </summary>
        protected virtual void Die()
        {
            isAlive = false;
            
            AudioManager.PlayEnemyDeathSound();
            
            Color effectColor = normalColor;
            SimpleVFX.CreateDeathEffect(transform.position, effectColor);
            
            ExperienceSystem expSystem = ExperienceSystem.Instance;
            if (expSystem != null)
            {
                // Detectar tipo de enemigo
                string enemyType = "cat"; // default
                if (this is Enemy.DogEnemy) enemyType = "dog";
               // if (this is Enemy.BossEnemy) enemyType = "boss"; // lo haremos después
        
                expSystem.OnEnemyKilled(enemyType);
            }

            // Notificar al WaveManager
            WaveManager waveManager = FindFirstObjectByType<WaveManager>();
            if (waveManager != null)
            {
                waveManager.OnEnemyDied(this);
            }

            // Destruir después de un pequeño delay
            Destroy(gameObject, 0.1f);
        }

        /// <summary>
        /// Implementación de IDamageable
        /// </summary>
        public bool IsAlive() => isAlive;

        /// <summary>
        /// Implementación de IDamageable
        /// </summary>
        public float GetCurrentHealth() => currentHealth;

        /// <summary>
        /// Dibuja el rango de ataque en el editor
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        // Getters y Setters
        public void SetMoveSpeed(float speed) => moveSpeed = speed;
        public void SetDamage(float dmg) => damage = dmg;
        public void SetMaxHealth(float health)
        {
            maxHealth = health;
            currentHealth = health;
        }
    }
}
