using UnityEditor;
using UnityEngine;
using Fusion;

public class AddNetworkObjectToCollectible
{
    [MenuItem("Tools/Add NetworkObject to Collectible Prefab")]
    public static void Execute()
    {
        string prefabPath = "Assets/Prefabs/CollectibleObjectPrefab.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError("Collectible prefab not found at: " + prefabPath);
            return;
        }

        using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
        {
            GameObject root = scope.prefabContentsRoot;

            if (root.GetComponent<NetworkObject>() != null)
            {
                Debug.Log("NetworkObject already present.");
                return;
            }

            root.AddComponent<NetworkObject>();
            Debug.Log("NetworkObject added to CollectibleObjectPrefab.");
        }
    }
}</content>
<parameter name="filePath">/Users/cs/Desktop/BattleSlime/Assets/Editor/AddNetworkObjectToCollectible.cs