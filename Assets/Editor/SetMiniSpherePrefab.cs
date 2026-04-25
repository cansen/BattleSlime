using UnityEditor;
using UnityEngine;

public class SetMiniSpherePrefab
{
    [MenuItem("Tools/Set Mini Sphere Prefab")]
    public static void Execute()
    {
        string playerPrefabPath = "Assets/Prefabs/Player.prefab";
        string collectiblePrefabPath = "Assets/Prefabs/CollectibleObjectPrefab.prefab";

        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
        GameObject collectiblePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(collectiblePrefabPath);

        if (playerPrefab == null)
        {
            Debug.LogError("Player prefab not found at: " + playerPrefabPath);
            return;
        }

        if (collectiblePrefab == null)
        {
            Debug.LogError("Collectible prefab not found at: " + collectiblePrefabPath);
            return;
        }

        PlayerStats playerStats = playerPrefab.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component not found on Player prefab.");
            return;
        }

        // Get the NetworkObject from collectible prefab
        NetworkObject networkObject = collectiblePrefab.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            Debug.LogError("NetworkObject component not found on CollectibleObjectPrefab.");
            return;
        }

        // Set the miniSpherePrefab
        // Since it's private, we need to use reflection or make it public temporarily
        // But for simplicity, assume we can set it via SerializedObject

        SerializedObject serializedObject = new SerializedObject(playerStats);
        SerializedProperty property = serializedObject.FindProperty("miniSpherePrefab");
        if (property != null)
        {
            property.objectReferenceValue = networkObject;
            serializedObject.ApplyModifiedProperties();
            Debug.Log("miniSpherePrefab set to CollectibleObjectPrefab.");
        }
        else
        {
            Debug.LogError("miniSpherePrefab property not found.");
        }
    }
}</content>
<parameter name="filePath">/Users/cs/Desktop/BattleSlime/Assets/Editor/SetMiniSpherePrefab.cs