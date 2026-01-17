using UnityEngine;

namespace PetsWars.PowerUps
{
    /// <summary>
    /// Power-up que otorga un escudo temporal
    /// </summary>
    public class ShieldPowerUp : PowerUpBase
    {
        [Header("Shield Power-Up")]
        [SerializeField] private float shieldDuration = 5f;
        [SerializeField] private Color shieldColor = Color.cyan;

        protected override void Awake()
        {
            powerUpColor = new Color(0.3f, 0.7f, 1f); // Azul claro
            base.Awake();
        }

        protected override void CollectPowerUp(GameObject player)
        {
            // Activar escudo temporal
            ShieldEffect shield = player.GetComponent<ShieldEffect>();
            if (shield == null)
            {
                shield = player.AddComponent<ShieldEffect>();
            }

            shield.ActivateShield(shieldDuration, shieldColor);
            Debug.Log($"🛡️ Escudo activado por {shieldDuration}s");

            base.CollectPowerUp(player);
        }
    }

    /// <summary>
    /// Componente que maneja el efecto de escudo temporal
    /// </summary>
    public class ShieldEffect : MonoBehaviour
    {
        private float shieldTimer;
        private bool isActive;
        private GameObject shieldVisual;
        private Player.PlayerHealth playerHealth;

        public void ActivateShield(float duration, Color color)
        {
            shieldTimer = duration;
            isActive = true;

            playerHealth = GetComponent<Player.PlayerHealth>();

            // Crear visual del escudo
            CreateShieldVisual(color);

            Debug.Log($"🛡️ ShieldEffect activado");
        }

        private void CreateShieldVisual(Color color)
        {
            // Crear sprite circular para el escudo
            if (shieldVisual == null)
            {
                shieldVisual = new GameObject("Shield");
                shieldVisual.transform.SetParent(transform);
                shieldVisual.transform.localPosition = Vector3.zero;
                shieldVisual.transform.localScale = Vector3.one * 1.5f;

                SpriteRenderer sr = shieldVisual.AddComponent<SpriteRenderer>();
                sr.sprite = CreateCircleSprite();
                sr.color = new Color(color.r, color.g, color.b, 0.3f);
                sr.sortingOrder = 10;
            }
        }

        private Sprite CreateCircleSprite()
        {
            Texture2D texture = new Texture2D(64, 64);
            Color[] pixels = new Color[64 * 64];

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(32, 32));
                    if (distance < 32 && distance > 28)
                    {
                        pixels[y * 64 + x] = Color.white;
                    }
                    else
                    {
                        pixels[y * 64 + x] = Color.clear;
                    }
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }

        private void Update()
        {
            if (!isActive) return;

            shieldTimer -= Time.deltaTime;

            // Rotación del escudo
            if (shieldVisual != null)
            {
                shieldVisual.transform.Rotate(0, 0, 100f * Time.deltaTime);
            }

            if (shieldTimer <= 0)
            {
                DeactivateShield();
            }
        }

        private void DeactivateShield()
        {
            isActive = false;
            if (shieldVisual != null)
            {
                Destroy(shieldVisual);
            }
            Debug.Log($"🛡️ Escudo desactivado");
        }

        /// <summary>
        /// Intercepta el daño mientras el escudo está activo
        /// </summary>
        public bool IsActive() => isActive;

        private void OnDestroy()
        {
            if (shieldVisual != null)
            {
                Destroy(shieldVisual);
            }
        }
    }
}