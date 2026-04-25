using Fusion;
using UnityEngine;

public class ObjectSpawner : NetworkBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private NetworkObject collectiblePrefab;
    [SerializeField] private int spawnCount = 30;
    [SerializeField] private float arenaHalfSize = 50f;

    [Header("Size Value Range")]
    [SerializeField] private float minSizeValue = 80f;
    [SerializeField] private float maxSizeValue = 500f;

    public override void Spawned()
    {
        if (!HasStateAuthority)
        {
            return;
        }
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
        float sizeValue = Random.Range(minSizeValue, maxSizeValue);
        float visualScale = Mathf.Pow(sizeValue, 1f / 3f);
        Vector3 position = GenerateRandomPosition();
        position.y = visualScale / 2f;
        Runner.Spawn(collectiblePrefab, position, Quaternion.identity,
            onBeforeSpawned: (runner, obj) => InitCollectible(obj, sizeValue, visualScale));
    }

    private void InitCollectible(NetworkObject obj, float sizeValue, float visualScale)
    {
        CollectibleObject co = obj.GetComponent<CollectibleObject>();
        if (co == null)
        {
            return;
        }
        co.objectSizeValue = sizeValue;
        co.canDamagePlayer = true;
        obj.transform.localScale = Vector3.one * visualScale;
    }

    private Vector3 GenerateRandomPosition()
    {
        float x = Random.Range(-arenaHalfSize, arenaHalfSize);
        float z = Random.Range(-arenaHalfSize, arenaHalfSize);
        return new Vector3(x, 0f, z);
    }
}
