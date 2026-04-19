using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject collectiblePrefab;
    [SerializeField] private int spawnCount = 30;
    [SerializeField] private float arenaHalfSize = 50f;
    [SerializeField] private float spawnHeightOffset = 0.5f;

    [Header("Size Value Range")]
    [SerializeField] private float minSizeValue = 0.2f;
    [SerializeField] private float maxSizeValue = 3f;

    private void Start()
    {
        SpawnObjects();
    }

    private void SpawnObjects()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnSingle();
        }
    }

    private void SpawnSingle()
    {
        Vector3 position = GenerateRandomPosition();
        GameObject instance = Instantiate(collectiblePrefab, position, Quaternion.identity);
        AssignSizeValue(instance);
    }

    private Vector3 GenerateRandomPosition()
    {
        float x = Random.Range(-arenaHalfSize, arenaHalfSize);
        float z = Random.Range(-arenaHalfSize, arenaHalfSize);
        return new Vector3(x, spawnHeightOffset, z);
    }

    private void AssignSizeValue(GameObject instance)
    {
        CollectibleObject collectible = instance.GetComponent<CollectibleObject>();
        if (collectible == null)
        {
            return;
        }

        float sizeValue = Random.Range(minSizeValue, maxSizeValue);
        collectible.objectSizeValue = sizeValue;
        instance.transform.localScale = Vector3.one * Mathf.Pow(sizeValue, 1f / 3f);
    }
}
