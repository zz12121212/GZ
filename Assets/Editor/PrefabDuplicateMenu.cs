using UnityEditor;
using UnityEngine;

public static class PrefabDuplicateMenu
{
    [MenuItem("GameObject/UI/CanvasPrefab", false)]
    static void CreatCanvas() => SpawnPrefab("Assets/Prefabs/UIPrefabs/Canvas.prefab");

    static void SpawnPrefab(string path) {
        var Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (!Prefab) {
            Debug.LogError("没有Canvas预制体");
            return;
        }
        var Obj = PrefabUtility.InstantiatePrefab(Prefab) as GameObject;
    }
}
