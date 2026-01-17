using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
    /// <summary>
    /// Controlador central de armas que maneja múltiples armas automáticas
    /// Dispara automáticamente al enemigo más cercano
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [Header("Weapon Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.5f; // Disparos por segundo
        [SerializeField] private int maxWeapons = 6;

        [Header("Projectile Settings")]
        [SerializeField] private float projectileDamage = 10f;
        [SerializeField] private float projectileSpeed = 12f;

        [Header("Targeting")]
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private LayerMask enemyLayer;

        private List<Weapon> weapons = new List<Weapon>();
        private float nextFireTime;
        private Transform currentTarget;

        // Clase interna para representar un arma
        private class Weapon
        {
            public float damage;
            public float fireRate;
            public float nextFireTime;
            public Vector2 offset;

            public Weapon(float dmg, float rate, Vector2 off)
            {
                damage = dmg;
                fireRate = rate;
                nextFireTime = 0f;
                offset = off;
            }
        }

        private void Start()
        {
            // Crear el punto de disparo si no existe
            if (firePoint == null)
            {
                GameObject fp = new GameObject("FirePoint");
                fp.transform.SetParent(transform);
                fp.transform.localPosition = new Vector3(0, 0.5f, 0);
                firePoint = fp.transform;
            }

            // Agregar arma inicial
            AddWeapon();
        }

        private void Update()
        {
            FindNearestEnemy();

            if (currentTarget != null)
            {
                FireWeapons();
            }
        }

        /// <summary>
        /// Encuentra el enemigo más cercano en el radio de detección
        /// </summary>
        private void FindNearestEnemy()
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);

            if (enemies.Length == 0)
            {
                currentTarget = null;
                return;
            }

            Transform closestEnemy = null;
            float closestDistance = Mathf.Infinity;

            foreach (Collider2D enemy in enemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy.transform;
                }
            }

            currentTarget = closestEnemy;
        }

        /// <summary>
        /// Dispara todas las armas que estén listas
        /// </summary>
        private void FireWeapons()
        {
            if (currentTarget == null) return;

            foreach (Weapon weapon in weapons)
            {
                if (Time.time >= weapon.nextFireTime)
                {
                    FireProjectile(weapon);
                    weapon.nextFireTime = Time.time + (1f / weapon.fireRate);
                }
            }
        }

        /// <summary>
        /// Dispara un proyectil desde un arma específica
        /// </summary>
        private void FireProjectile(Weapon weapon)
        {
            if (projectilePrefab == null)
            {
                Debug.LogError("Projectile Prefab no asignado en WeaponController");
                return;
            }

            // Calcular posición de disparo con offset
            Vector3 spawnPosition = firePoint.position + (Vector3)weapon.offset;

            // Instanciar proyectil
            GameObject proj = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);

            // Calcular dirección hacia el objetivo
            Vector2 direction = (currentTarget.position - spawnPosition).normalized;

            // Inicializar proyectil
            Projectile projectileScript = proj.GetComponent<Projectile>();
            if (projectileScript != null)
            {
                projectileScript.Initialize(direction, weapon.damage, projectileSpeed);
            }
        }

        /// <summary>
        /// Agrega un arma nueva al arsenal
        /// </summary>
        public bool AddWeapon()
        {
            if (weapons.Count >= maxWeapons)
            {
                Debug.Log("Máximo de armas alcanzado");
                return false;
            }

            // Calcular offset circular para múltiples armas
            float angle = (weapons.Count * 360f / maxWeapons) * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.3f;

            Weapon newWeapon = new Weapon(projectileDamage, fireRate, offset);
            weapons.Add(newWeapon);

            Debug.Log($"Arma agregada. Total: {weapons.Count}");
            return true;
        }

        /// <summary>
        /// Mejora el daño de todas las armas
        /// </summary>
        public void UpgradeDamage(float multiplier)
        {
            projectileDamage *= multiplier;
            foreach (Weapon weapon in weapons)
            {
                weapon.damage *= multiplier;
            }
        }

        /// <summary>
        /// Mejora la cadencia de todas las armas
        /// </summary>
        public void UpgradeFireRate(float multiplier)
        {
            fireRate *= multiplier;
            foreach (Weapon weapon in weapons)
            {
                weapon.fireRate *= multiplier;
            }
        }

        /// <summary>
        /// Dibuja el radio de detección en el editor
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

        // Getters públicos
        public int GetWeaponCount() => weapons.Count;
        public float GetCurrentDamage() => projectileDamage;
        public float GetCurrentFireRate() => fireRate;
    }
}
