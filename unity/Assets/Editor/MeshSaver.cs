using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class MeshSaver
{
    [MenuItem("GameObject/Save As Prefab (incl. Meshes)")]
    public static void SaveMesh()
    {
        if (!Selection.activeGameObject)
            return;

        string path = EditorUtility.SaveFilePanel("Save As Prefab", "Assets/", "Saved Object", "prefab");
        if (string.IsNullOrEmpty(path)) return;

        path = FileUtil.GetProjectRelativePath(path);

        var meshFiltersOriginal = Selection.activeGameObject.GetComponentsInChildren<MeshFilter>();
        Dictionary<string, Mesh> meshLookup = new Dictionary<string, Mesh>();
        for (int i = 0; i < meshFiltersOriginal.Length; i++)
        {
            meshLookup.Add(GetPath(meshFiltersOriginal[i].gameObject), meshFiltersOriginal[i].mesh);
        }


        var savedObject = PrefabUtility.SaveAsPrefabAsset(Selection.activeGameObject, path);
        var meshFiltersInSaveObj = savedObject.GetComponentsInChildren<MeshFilter>();
        for (int i = 0; i < meshFiltersInSaveObj.Length; i++)
        {
            var mesh = meshLookup[GetPath(meshFiltersInSaveObj[i].gameObject)];

            meshFiltersInSaveObj[i].sharedMesh = mesh;
            meshFiltersInSaveObj[i].GetComponent<MeshCollider>().sharedMesh = mesh;
            AssetDatabase.AddObjectToAsset(mesh, path);
        }

        AssetDatabase.SaveAssets();
    }

    static string GetPath(GameObject obj)
    {
        return obj.name;
    }
}