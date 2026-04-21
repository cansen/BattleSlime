using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject collectiblePrefab;
    [SerializeField] private int spawnCount = 30;
    [SerializeField] private float arenaHalfSize = 50f;

    [Header("Size Value Range")]
    [SerializeField] private float minSizeValue = 10f;
    [SerializeField] private float maxSizeValue = 200f;

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
        return new Vector3(x, 0f, z);
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
        float visualScale = Mathf.Pow(sizeValue, 1f / 3f);
        instance.transform.localScale = Vector3.one * visualScale;

        Vector3 position = instance.transform.position;
        position.y = visualScale / 2f;
        instance.transform.position = position;
    }
}
