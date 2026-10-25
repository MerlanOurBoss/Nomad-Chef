using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NomadChef.EditorTools
{
    /// <summary>
    /// Connects models exported from ArtSource/ (Blender) to the Unity colour palette.
    /// Every Blender material named "Foo" is remapped to Assets/Art/Materials/M_Art_Foo.mat,
    /// which is created (grey) if it does not exist yet. Remaps are stored in each model's
    /// .meta, so re-exporting from Blender keeps them; run this again only for new materials.
    /// </summary>
    public static class ArtPaletteTool
    {
        private const string ModelsFolder = "Assets/Art/Models";
        private const string MaterialsFolder = "Assets/Art/Materials";
        private const string Prefix = "M_Art_";

        [MenuItem("Nomad Chef/Art/Apply Palette Materials")]
        public static void ApplyPaletteMaterials()
        {
            int remapped = 0;
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { ModelsFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = (ModelImporter)AssetImporter.GetAtPath(path);

                importer.importAnimation = false;
                importer.animationType = ModelImporterAnimationType.None;
                importer.importCameras = false;
                importer.importLights = false;
                importer.importBlendShapes = false;

                foreach (string materialName in SourceMaterialNames(path))
                {
                    importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), materialName),
                        GetOrCreatePaletteMaterial(materialName));
                    remapped++;
                }

                importer.SaveAndReimport();
            }

            Debug.Log($"Nomad Chef art: remapped {remapped} material slots to the palette.");
        }

        private static IEnumerable<string> SourceMaterialNames(string modelPath)
        {
            var names = new HashSet<string>();
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(modelPath))
            {
                if (asset is Renderer renderer)
                {
                    foreach (Material material in renderer.sharedMaterials)
                    {
                        if (material != null)
                            names.Add(material.name.StartsWith(Prefix) ? material.name.Substring(Prefix.Length) : material.name);
                    }
                }
            }
            return names;
        }

        private static Material GetOrCreatePaletteMaterial(string materialName)
        {
            string path = $"{MaterialsFolder}/{Prefix}{materialName}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            if (!AssetDatabase.IsValidFolder(MaterialsFolder))
                AssetDatabase.CreateFolder("Assets/Art", "Materials");

            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = Prefix + materialName };
            material.SetColor("_BaseColor", Color.grey);
            AssetDatabase.CreateAsset(material, path);
            Debug.LogWarning($"Nomad Chef art: created placeholder palette material {path}; set its colour.", material);
            return material;
        }
    }
}
