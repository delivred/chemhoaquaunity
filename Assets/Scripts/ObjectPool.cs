using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object Pool đơn giản, tái sử dụng GameObject thay vì Instantiate/Destroy liên tục.
/// Giúp game mượt hơn khi spawn nhiều trái cây/bom liên tục.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size = 10;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectQueue = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectQueue.Enqueue(obj);
            }
            poolDictionary.Add(pool.tag, objectQueue);
        }
    }

    /// <summary>Lấy 1 object từ pool ra sử dụng tại vị trí/góc xoay chỉ định.</summary>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool với tag '{tag}' không tồn tại.");
            return null;
        }

        Queue<GameObject> queue = poolDictionary[tag];

        GameObject objectToSpawn;
        if (queue.Count == 0)
        {
            // Nếu hết object rảnh, tự tạo thêm 1 cái (tránh thiếu hụt khi cần nhiều)
            Pool pool = pools.Find(p => p.tag == tag);
            objectToSpawn = Instantiate(pool.prefab, transform);
        }
        else
        {
            objectToSpawn = queue.Dequeue();
        }

        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;
        objectToSpawn.SetActive(true);

        return objectToSpawn;
    }

    /// <summary>Trả object về lại pool sau khi dùng xong (thay vì Destroy).</summary>
    public void ReturnToPool(string tag, GameObject obj)
    {
        obj.SetActive(false);
        if (poolDictionary.ContainsKey(tag))
        {
            poolDictionary[tag].Enqueue(obj);
        }
    }
}
