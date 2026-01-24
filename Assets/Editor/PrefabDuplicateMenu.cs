using UnityEditor;
using UnityEngine;

public static class PrefabDuplicateMenu
{
    [MenuItem("GameObject/UI/PanelPrefab", false)]
    static void CreatPanel() => SpawnPrefab("Assets/Prefabs/UIPrefabs/Panel.prefab");

    static void SpawnPrefab(string path) {
        var Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (!Prefab) {
            Debug.LogError("没有Panel预制体");
            return;
        }
        var Obj = PrefabUtility.InstantiatePrefab(Prefab) as GameObject;
    }
}
