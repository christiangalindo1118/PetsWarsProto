using System.Collections.Generic;
using UnityEngine;

namespace PetsWars.Utilities
{
    /// <summary>
    /// Sistema de pooling de objetos para mejorar performance
    /// Reutiliza proyectiles y efectos en lugar de crearlos/destruirlos constantemente
    /// </summary>
    public class ObjectPooler : MonoBehaviour
    {
        [System.Serializable]
        public class Pool
        {
            public string tag;
            public GameObject prefab;
            public int initialSize;
        }

        [Header("Pools Configuration")]
        [SerializeField] private List<Pool> pools = new List<Pool>();

        private Dictionary<string, Queue<GameObject>> poolDictionary;
        private static ObjectPooler instance;

        public static ObjectPooler Instance => instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializePools();
        }

        /// <summary>
        /// Inicializa todos los pools
        /// </summary>
        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                // Pre-instanciar objetos
                for (int i = 0; i < pool.initialSize; i++)
                {
                    GameObject obj = CreateNewObject(pool.prefab);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(pool.tag, objectPool);
            }
        }

        /// <summary>
        /// Crea un nuevo objeto y lo desactiva
        /// </summary>
        private GameObject CreateNewObject(GameObject prefab)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            return obj;
        }

        /// <summary>
        /// Obtiene un objeto del pool
        /// </summary>
        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool con tag {tag} no existe");
                return null;
            }

            GameObject obj;

            // Si el pool está vacío, crear nuevo objeto
            if (poolDictionary[tag].Count == 0)
            {
                Pool pool = pools.Find(p => p.tag == tag);
                obj = CreateNewObject(pool.prefab);
            }
            else
            {
                obj = poolDictionary[tag].Dequeue();
            }

            obj.SetActive(true);
            obj.transform.position = position;
            obj.transform.rotation = rotation;

            return obj;
        }

        /// <summary>
        /// Devuelve un objeto al pool
        /// </summary>
        public void ReturnToPool(string tag, GameObject obj)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Pool con tag {tag} no existe");
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            poolDictionary[tag].Enqueue(obj);
        }

        /// <summary>
        /// Limpia todos los pools
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in poolDictionary.Values)
            {
                while (pool.Count > 0)
                {
                    GameObject obj = pool.Dequeue();
                    Destroy(obj);
                }
            }

            poolDictionary.Clear();
        }
    }
}
