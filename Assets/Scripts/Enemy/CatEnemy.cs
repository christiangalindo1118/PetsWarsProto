using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// Enemigo Gato - Rápido y con poca vida
    /// Estrategia: Ataque veloz en enjambre
    /// </summary>
    public class CatEnemy : EnemyBase
    {
        [Header("Cat Specific")]
        [SerializeField] private float dashSpeed = 8f;
        [SerializeField] private float dashCooldown = 3f;
        [SerializeField] private float dashDuration = 0.3f;

        private float lastDashTime;
        private bool isDashing;
        private float dashTimer;

        protected override void Start()
        {
            base.Start();

            // Stats específicos del gato
            maxHealth = 30f;
            currentHealth = maxHealth;
            moveSpeed = 4f;
            damage = 8f;
            attackRange = 0.8f;
            attackCooldown = 0.8f;

            normalColor = new Color(0.7f, 0.7f, 0.7f); // Gris
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
        }

        protected override void Update()
        {
            base.Update();

            if (!isAlive || player == null) return;

            // Manejar dash
            if (isDashing)
            {
                dashTimer -= Time.deltaTime;
                if (dashTimer <= 0)
                {
                    isDashing = false;
                }
            }
            else
            {
                // Intentar dash si está cerca del jugador
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);
                if (distanceToPlayer <= 5f && distanceToPlayer > 2f && Time.time >= lastDashTime + dashCooldown)
                {
                    StartDash();
                }
            }
        }

        protected override void MoveTowardsPlayer()
        {
            if (isDashing)
            {
                // Movimiento más rápido durante el dash
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * dashSpeed;
            }
            else
            {
                base.MoveTowardsPlayer();
            }
        }

        private void StartDash()
        {
            isDashing = true;
            dashTimer = dashDuration;
            lastDashTime = Time.time;
        }

        protected override void Die()
        {
            // Efectos especiales de muerte para gato
            Debug.Log("¡Gato derrotado!");
            base.Die();
        }
    }
}
