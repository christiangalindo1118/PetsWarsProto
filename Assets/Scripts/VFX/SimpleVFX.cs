using UnityEngine;
using UnityEngine.UI;

namespace PetsWars.VFX
{
    /// <summary>
    /// Sistema simple de efectos visuales
    /// Crea efectos de partículas básicos para feedback visual
    /// </summary>
    public class SimpleVFX : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject deathEffectPrefab;

        [Header("Settings")]
        [SerializeField] private bool useSimpleEffects = true;

        private static SimpleVFX instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Crea un efecto de impacto
        /// </summary>
        public static void CreateHitEffect(Vector3 position, Color color)
        {
            if (instance == null) return;

            // Si hay prefab personalizado, usarlo
            if (instance.hitEffectPrefab != null)
            {
                Instantiate(instance.hitEffectPrefab, position, Quaternion.identity);
                return;
            }

            // Si no, crear efecto simple
            if (instance.useSimpleEffects)
            {
                CreateSimpleHitEffect(position, color);
            }
        }

        /// <summary>
        /// Crea un efecto de muerte
        /// </summary>
        public static void CreateDeathEffect(Vector3 position, Color color)
        {
            if (instance == null) return;

            // Si hay prefab personalizado, usarlo
            if (instance.deathEffectPrefab != null)
            {
                Instantiate(instance.deathEffectPrefab, position, Quaternion.identity);
                return;
            }

            // Si no, crear efecto simple (explosión de partículas)
            if (instance.useSimpleEffects)
            {
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * 45f * Mathf.Deg2Rad;
                    Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                    Vector3 spawnPos = position + direction * 0.5f;

                    CreateSimpleHitEffect(spawnPos, color);
                }
            }
        }

        /// <summary>
        /// Crea un efecto de impacto simple usando sprites
        /// </summary>
        private static void CreateSimpleHitEffect(Vector3 position, Color color)
        {
            GameObject effect = new GameObject("HitEffect");
            effect.transform.position = position;

            SpriteRenderer sprite = effect.AddComponent<SpriteRenderer>();
            sprite.sprite = CreateCircleSprite();
            sprite.color = color;
            sprite.sortingOrder = 100;

            // Animación simple
            HitEffectAnimator animator = effect.AddComponent<HitEffectAnimator>();
            animator.Initialize(0.3f);
        }

        /// <summary>
        /// Crea un sprite circular simple
        /// </summary>
        private static Sprite CreateCircleSprite()
        {
            Texture2D texture = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];

            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                    pixels[y * 32 + x] = distance < 16 ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Efecto de pantalla al recibir daño
        /// </summary>
        public static void ScreenFlash(Color color, float duration = 0.2f)
        {
            if (instance != null)
            {
                instance.StartCoroutine(instance.ScreenFlashCoroutine(color, duration));
            }
        }

        private System.Collections.IEnumerator ScreenFlashCoroutine(Color color, float duration)
        {
            // Crear overlay temporal
            GameObject overlay = new GameObject("ScreenFlash");
            Canvas canvas = overlay.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;

            UnityEngine.UI.Image image = overlay.AddComponent<UnityEngine.UI.Image>();
            image.color = color;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / duration);
                Color c = image.color;
                c.a = alpha;
                image.color = c;
                yield return null;
            }

            Destroy(overlay);
        }
    }

    /// <summary>
    /// Anima efectos de impacto simples
    /// </summary>
    public class HitEffectAnimator : MonoBehaviour
    {
        private float lifetime;
        private float timer;
        private SpriteRenderer sprite;
        private Vector3 initialScale;

        public void Initialize(float duration)
        {
            lifetime = duration;
            timer = 0;
            sprite = GetComponent<SpriteRenderer>();
            initialScale = transform.localScale;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float progress = timer / lifetime;

            // Fade out
            if (sprite != null)
            {
                Color color = sprite.color;
                color.a = 1f - progress;
                sprite.color = color;
            }

            // Scale up
            transform.localScale = initialScale * (1f + progress * 2f);

            // Destruir al terminar
            if (progress >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}