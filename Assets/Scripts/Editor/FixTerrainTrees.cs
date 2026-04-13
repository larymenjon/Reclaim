using UnityEditor;
using UnityEngine;

public class FixTerrainTrees : MonoBehaviour
{
    [MenuItem("Tools/Fix Terrain Trees")]
    public static void FixMissingTreePrefabs()
    {
        string terrainPath = "Assets/Imports/Free Island Collection/Environment/Terrain/Terrains/Terrain 1.asset";
        Terrain terrain = AssetDatabase.LoadAssetAtPath<Terrain>(terrainPath);
        
        if (terrain == null)
        {
            Debug.LogError("Terrain not found at path: " + terrainPath);
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        if (terrainData == null)
        {
            Debug.LogError("TerrainData not found!");
            return;
        }

        // Remove all tree prototypes to clear the errors
        TerrainLayer[] layers = terrainData.terrainLayers;
        
        // Get current tree prototypes
        TreePrototype[] currentTrees = terrainData.treePrototypes;
        Debug.Log($"Found {currentTrees.Length} tree prototypes");

        // Clear all tree instances
        terrainData.SetTreeInstances(new TreeInstance[0], true);
        
        // Clear all tree prototypes
        terrainData.treePrototypes = new TreePrototype[0];

        // Mark as modified
        EditorUtility.SetDirty(terrainData);
        AssetDatabase.SaveAssets();

        Debug.Log("✅ Successfully removed all tree prototypes from Terrain 1!");
        Debug.Log("All missing tree references have been cleared.");
    }
}
