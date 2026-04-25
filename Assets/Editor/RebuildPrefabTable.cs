using UnityEngine;
using UnityEditor;
using Fusion.Editor;

public class RebuildPrefabTable
{
    public static void Execute()
    {
        NetworkProjectConfigUtilities.RebuildPrefabTable();
        Debug.Log("[RebuildPrefabTable] Done.");
    }
}
