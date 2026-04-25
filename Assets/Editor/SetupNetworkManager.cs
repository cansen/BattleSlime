using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Fusion;

public class SetupNetworkManager
{
    public static void Execute()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("[SetupNetworkManager] Stop Play mode first.");
            return;
        }

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == "NetworkManager")
            {
                Object.DestroyImmediate(root);
                Debug.Log("[SetupNetworkManager] Removed existing NetworkManager.");
                break;
            }
        }

        GameObject go = new GameObject("NetworkManager");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, scene);

        go.AddComponent<NetworkManager>();

        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        System.Type physicsType = null;
        foreach (var asm in assemblies)
        {
            physicsType = asm.GetType("Fusion.Addons.Physics.RunnerSimulatePhysics3D");
            if (physicsType != null) break;
        }
        if (physicsType != null)
        {
            go.AddComponent(physicsType);
        }
        else
        {
            Debug.LogWarning("[SetupNetworkManager] RunnerSimulatePhysics3D not found — add it manually.");
        }

        NetworkManager nm = go.GetComponent<NetworkManager>();
        SerializedObject so = new SerializedObject(nm);

        string playerPrefabPath = "Assets/Photon/Fusion/Prefabs/Player.prefab";
        NetworkObject playerPrefab = AssetDatabase.LoadAssetAtPath<NetworkObject>(playerPrefabPath);
        if (playerPrefab != null)
        {
            so.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
            so.ApplyModifiedProperties();
            Debug.Log("[SetupNetworkManager] playerPrefab wired: " + playerPrefabPath);
        }
        else
        {
            Debug.LogWarning("[SetupNetworkManager] Player prefab not found at: " + playerPrefabPath);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SetupNetworkManager] Done. NetworkManager added to SampleScene.");
    }
}
