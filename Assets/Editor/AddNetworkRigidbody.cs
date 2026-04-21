using UnityEditor;
using UnityEngine;
using Fusion;
using Fusion.Addons.Physics;

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

            NetworkTransform nt = root.GetComponent<NetworkTransform>();
            if (nt != null)
            {
                Object.DestroyImmediate(nt);
                Debug.Log("Removed NetworkTransform.");
            }

            if (root.GetComponent<NetworkRigidbody3D>() != null)
            {
                Debug.Log("NetworkRigidbody3D already present.");
                return;
            }

            root.AddComponent<NetworkRigidbody3D>();
            Debug.Log("NetworkRigidbody3D added to Player prefab.");
        }
    }
}
