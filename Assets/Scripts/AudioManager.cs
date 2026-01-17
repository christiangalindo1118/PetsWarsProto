using UnityEngine;

namespace PetsWars
{
    /// <summary>
    /// Gestor simple de audio para el juego
    /// Maneja música de fondo y efectos de sonido
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Music")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private float musicVolume = 0.5f;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip shootSound;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip enemyDeathSound;
        [SerializeField] private AudioClip playerHitSound;
        [SerializeField] private float sfxVolume = 0.7f;

        private static AudioManager instance;

        private void Awake()
        {
            // Singleton
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Crear AudioSources si no existen
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void Start()
        {
            PlayMusic();
        }

        /// <summary>
        /// Reproduce la música de fondo
        /// </summary>
        public void PlayMusic()
        {
            if (musicSource != null && backgroundMusic != null)
            {
                musicSource.clip = backgroundMusic;
                musicSource.volume = musicVolume;
                musicSource.Play();
            }
        }

        /// <summary>
        /// Detiene la música
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// Reproduce un efecto de sonido
        /// </summary>
        public static void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (instance == null || instance.sfxSource == null || clip == null) return;

            instance.sfxSource.PlayOneShot(clip, instance.sfxVolume * volumeMultiplier);
        }

        /// <summary>
        /// Reproduce sonido de disparo
        /// </summary>
        public static void PlayShootSound()
        {
            if (instance != null && instance.shootSound != null)
            {
                PlaySFX(instance.shootSound, 0.5f);
            }
        }

        /// <summary>
        /// Reproduce sonido de impacto
        /// </summary>
        public static void PlayHitSound()
        {
            if (instance != null && instance.hitSound != null)
            {
                PlaySFX(instance.hitSound);
            }
        }

        /// <summary>
        /// Reproduce sonido de muerte de enemigo
        /// </summary>
        public static void PlayEnemyDeathSound()
        {
            if (instance != null && instance.enemyDeathSound != null)
            {
                PlaySFX(instance.enemyDeathSound);
            }
        }

        /// <summary>
        /// Reproduce sonido de jugador recibiendo daño
        /// </summary>
        public static void PlayPlayerHitSound()
        {
            if (instance != null && instance.playerHitSound != null)
            {
                PlaySFX(instance.playerHitSound);
            }
        }

        /// <summary>
        /// Ajusta el volumen de la música
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
        }

        /// <summary>
        /// Ajusta el volumen de efectos de sonido
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }
    }
}
