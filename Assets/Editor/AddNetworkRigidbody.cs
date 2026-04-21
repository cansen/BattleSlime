using UnityEditor;
using UnityEngine;
using Fusion;

public class AddNetworkRigidbody
{
    public static void Execute()
    {
        string prefabPath = "Assets/Prefabs/Player.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            Debug.LogError("Player prefab not found at: " + prefabPath);
            return;
        }

        using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
        {
            GameObject root = scope.prefabContentsRoot;

            if (root.GetComponent<NetworkTransform>() != null)
            {
                Debug.Log("NetworkTransform already present.");
                return;
            }

            root.AddComponent<NetworkTransform>();
            Debug.Log("NetworkTransform added to Player prefab.");
        }
    }
}
