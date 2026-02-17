using UnityEditor;
using UnityEngine;
using System.IO;

public class CreateMaterialsFromTextures
{
    [MenuItem("Tools/Create Materials From Selected Textures")]
    static void CreateMaterials()
    {
        foreach (var obj in Selection.objects)
        {
            if (obj is Texture2D texture)
            {
                string texturePath = AssetDatabase.GetAssetPath(texture);
                string folder = Path.GetDirectoryName(texturePath);

                string materialsFolder = Path.Combine(folder, "Materials");

                if (!AssetDatabase.IsValidFolder(materialsFolder))
                {
                    AssetDatabase.CreateFolder(folder, "Materials");
                }

                string matPath = Path.Combine(materialsFolder, texture.name + ".mat");

                Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                mat.mainTexture = texture;

                AssetDatabase.CreateAsset(mat, matPath);
            }
        }

        AssetDatabase.Refresh();
    }
}
