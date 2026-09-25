using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sinh trái cây và bom liên tục trong lúc chơi, bay lên từ đáy màn hình theo quỹ đạo parabol.
/// Gắn script này vào 1 GameObject rỗng tên "Spawner" trong scene.
/// </summary>
public class FruitSpawner : MonoBehaviour
{
    [Header("Prefabs trái cây thường")]
    public List<GameObject> normalFruitPrefabs;

    [Header("Prefabs đặc biệt")]
    public GameObject goldenFruitPrefab;
    public GameObject freezeFruitPrefab;
    public GameObject bombPrefab;

    [Header("Tỉ lệ xuất hiện (tổng nên ~1.0)")]
    [Range(0f, 1f)] public float goldenChance = 0.05f;
    [Range(0f, 1f)] public float freezeChance = 0.05f;
    [Range(0f, 1f)] public float bombChance = 0.12f;

    [Header("Thời gian spawn")]
    public float minSpawnInterval = 0.4f;
    public float maxSpawnInterval = 1.2f;

    [Header("Lực bắn (quỹ đạo bay lên)")]
    public float minForceX = -2f;
    public float maxForceX = 2f;
    public float minForceY = 12f;
    public float maxForceY = 16f;

    [Header("Vị trí spawn")]
    public float spawnY = -6f;
    public float spawnXRange = 4f;

    [Header("Số lượng quả bắn cùng lúc (tăng dần theo độ khó)")]
    public int minBurst = 1;
    public int maxBurst = 3;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // Chỉ spawn khi đang chơi
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameManager.GameState.Playing)
            {
                float difficulty = GameManager.Instance.currentDifficultyFactor;

                int burst = Random.Range(minBurst, maxBurst + 1);
                for (int i = 0; i < burst; i++)
                {
                    SpawnOne();
                }

                float interval = Random.Range(minSpawnInterval, maxSpawnInterval) / difficulty;
                yield return new WaitForSeconds(Mathf.Max(0.15f, interval));
            }
            else
            {
                yield return null;
            }
        }
    }

    private void SpawnOne()
    {
        GameObject prefabToSpawn = ChoosePrefab();
        if (prefabToSpawn == null) return;

        Vector3 spawnPos = new Vector3(Random.Range(-spawnXRange, spawnXRange), spawnY, 0f);
        GameObject obj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        Vector2 force = new Vector2(Random.Range(minForceX, maxForceX), Random.Range(minForceY, maxForceY));
        float torque = Random.Range(-3f, 3f);

        Fruit fruit = obj.GetComponent<Fruit>();
        if (fruit != null)
        {
            fruit.Launch(force, torque);
            return;
        }

        Bomb bomb = obj.GetComponent<Bomb>();
        if (bomb != null)
        {
            bomb.Launch(force, torque);
        }
    }

    private GameObject ChoosePrefab()
    {
        float roll = Random.value;

        if (roll < bombChance && bombPrefab != null)
            return bombPrefab;

        roll -= bombChance;
        if (roll < goldenChance && goldenFruitPrefab != null)
            return goldenFruitPrefab;

        roll -= goldenChance;
        if (roll < freezeChance && freezeFruitPrefab != null)
            return freezeFruitPrefab;

        if (normalFruitPrefabs != null && normalFruitPrefabs.Count > 0)
            return normalFruitPrefabs[Random.Range(0, normalFruitPrefabs.Count)];

        return null;
    }
}
