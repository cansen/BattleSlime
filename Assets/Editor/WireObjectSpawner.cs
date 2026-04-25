using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Fusion;

public class WireObjectSpawner
{
    public static void Execute()
    {
        string prefabPath = "Assets/Prefabs/CollectibleObjectPrefab.prefab";

        // Ensure NetworkObject is on the prefab root
        GameObject prefabGo = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefabGo == null) { Debug.LogError("[WireObjectSpawner] Prefab not found."); return; }

        NetworkObject no = prefabGo.GetComponent<NetworkObject>();
        if (no == null)
        {
            prefabGo.AddComponent<NetworkObject>();
            PrefabUtility.SavePrefabAsset(prefabGo);
            AssetDatabase.Refresh();
            prefabGo = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            no = prefabGo.GetComponent<NetworkObject>();
            Debug.Log("[WireObjectSpawner] Added NetworkObject to CollectibleObjectPrefab.");
        }

        if (no == null) { Debug.LogError("[WireObjectSpawner] NetworkObject still null after save."); return; }

        // Wire ObjectSpawner in scene
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
        {
            ObjectSpawner spawner = root.GetComponent<ObjectSpawner>();
            if (spawner == null) continue;

            SerializedObject so = new SerializedObject(spawner);
            so.FindProperty("collectiblePrefab").objectReferenceValue = no;
            so.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[WireObjectSpawner] collectiblePrefab wired.");
            return;
        }
        Debug.LogError("[WireObjectSpawner] ObjectSpawner not found in scene.");
    }
}
