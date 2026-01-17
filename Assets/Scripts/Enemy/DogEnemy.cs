using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// Enemigo Perro Golden Retriever - Tanque resistente
    /// Estrategia: Alta vida, movimiento lento pero mucho daño
    /// </summary>
    public class DogEnemy : EnemyBase
    {
        [Header("Dog Specific")]
        [SerializeField] private float armor = 0.2f; // Reducción de daño 20%

        protected override void Start()
        {
            base.Start();

            // Stats específicos del perro
            maxHealth = 80f;
            currentHealth = maxHealth;
            moveSpeed = 2f;
            damage = 15f;
            attackRange = 1.2f;
            attackCooldown = 1.5f;

            normalColor = new Color(1f, 0.84f, 0f); // Dorado
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
        }

        public override void TakeDamage(float damageAmount)
        {
            // Aplicar armadura (reducir daño)
            float reducedDamage = damageAmount * (1f - armor);
            base.TakeDamage(reducedDamage);
        }

        protected override void Attack()
        {
            base.Attack();

            // El perro hace un pequeño empujón al atacar
            if (player != null)
            {
                Vector2 knockbackDirection = (player.position - transform.position).normalized;
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);
                }
            }
        }

        protected override void Die()
        {
            Debug.Log("¡Golden Retriever derrotado!");
            base.Die();
        }
    }
}
