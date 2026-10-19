using Reclaim.Construction;
using Reclaim.Settlement;
using UnityEditor;
using UnityEngine;

namespace Reclaim.EditorTools
{
    /// <summary>
    /// Garante o asset Assets/Resources/SettlementSettings, onde ficam os espaços de prefab de homem,
    /// mulher, criança, cachorro e barraca. Criado sozinho na primeira vez e também pelo menu
    /// Reclaim > Criar configuração do assentamento (que apenas seleciona o asset se ele já existe).
    /// </summary>
    [InitializeOnLoad]
    public static class SettlementSettingsCreator
    {
        private const string FolderPath = "Assets/Resources";
        private const string AssetPath = "Assets/Resources/SettlementSettings.asset";

        static SettlementSettingsCreator()
        {
            EditorApplication.delayCall += () => EnsureAsset(false);
        }

        [MenuItem("Reclaim/Criar configuração do assentamento")]
        public static void CreateFromMenu()
        {
            EnsureAsset(true);
        }

        private static void EnsureAsset(bool select)
        {
            SettlementSettings settings = AssetDatabase.LoadAssetAtPath<SettlementSettings>(AssetPath);
            if (settings == null)
            {
                if (!AssetDatabase.IsValidFolder(FolderPath))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }

                settings = ScriptableObject.CreateInstance<SettlementSettings>();
                AssignDefaults(settings);
                AssetDatabase.CreateAsset(settings, AssetPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[Reclaim] Criado " + AssetPath + ". Arraste os prefabs de homem, mulher, criança, cachorro e barraca nele.");
            }

            if (select)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
        }

        /// <summary>Liga o que o projeto já tem: árvore e toco, armazém e oficina.</summary>
        private static void AssignDefaults(SettlementSettings settings)
        {
            SerializedObject serialized = new SerializedObject(settings);
            SetReference(serialized, "treeStandingPrefab", "Assets/Prefabs/Arvores/arvore_tronco_01.prefab");
            SetReference(serialized, "treeStumpPrefab", "Assets/Prefabs/Arvores/cepo_tronco_01.prefab");
            SetReference(serialized, "storageBuilding", "Assets/Data/Storage.asset");
            SetReference(serialized, "industryBuilding", "Assets/Data/Industry.asset");
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetReference(SerializedObject serialized, string field, string path)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            SerializedProperty property = serialized.FindProperty(field);
            if (asset != null && property != null)
            {
                property.objectReferenceValue = asset;
            }
        }
    }
}
