using Reclaim.Audio;
using Reclaim.Calendar;
using Reclaim.Climate;
using Reclaim.Construction;
using Reclaim.Economy;
using Reclaim.Economy.Forestry;
using Reclaim.Events;
using Reclaim.Gameplay;
using Reclaim.Grid;
using Reclaim.Input;
using Reclaim.Morale;
using Reclaim.Population;
using Reclaim.Road;
using Reclaim.Simulation;
using Reclaim.UI.Bottom;
using Reclaim.UI.Hud;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Reclaim.EditorTools
{
    /// <summary>
    /// Menu Reclaim > Configurar cena de jogo. Cria o objeto "Simulação" com todos os sistemas
    /// da colônia, aplica os dados padrão de Assets/Data/Config e liga as referências dos
    /// componentes que já existem na cena aberta. Pode ser executado de novo sem duplicar nada.
    /// </summary>
    public static class GameSceneSetupTool
    {
        private const string SimulationObjectName = "Simulação";
        private const string ConfigFolder = "Assets/Data/Config";

        [MenuItem("Reclaim/Configurar cena de jogo")]
        public static void ConfigureOpenScene()
        {
            GameObject root = FindOrCreateRoot();

            SimulationClock clock = Ensure<SimulationClock>(root);
            DayNightCycle dayNight = Ensure<DayNightCycle>(root);
            ClimateSystem climate = Ensure<ClimateSystem>(root);
            ResourceStock stock = Ensure<ResourceStock>(root);
            FamilyManager families = Ensure<FamilyManager>(root);
            NeedsSystem needs = Ensure<NeedsSystem>(root);
            MoraleSystem morale = Ensure<MoraleSystem>(root);
            EventManager events = Ensure<EventManager>(root);
            ColonySimulation simulation = Ensure<ColonySimulation>(root);

            AssignDefaultData(climate, stock, families);
            Wire(dayNight, "clock", clock);
            Wire(simulation, "clock", clock);
            Wire(simulation, "climate", climate);
            Wire(simulation, "resourceStock", stock);
            Wire(simulation, "familyManager", families);
            Wire(simulation, "needsSystem", needs);
            Wire(simulation, "moraleSystem", morale);
            Wire(simulation, "eventManager", events);

            WireSceneComponents(simulation, dayNight, stock, families);

            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = root;
            Debug.Log("[Reclaim] Cena configurada. Confira no Console se algum componente ficou sem referência.");
        }

        private static void WireSceneComponents(ColonySimulation simulation, DayNightCycle dayNight, ResourceStock stock, FamilyManager families)
        {
            GridManager grid = FindInScene<GridManager>();
            TreeForestrySystem forestry = FindInScene<TreeForestrySystem>();
            BuildingSystem buildings = FindInScene<BuildingSystem>();
            ToolModeController tools = FindInScene<ToolModeController>();
            RoadBuilderSystem roadSystem = FindInScene<RoadBuilderSystem>();
            RoadBuilderManager roadManager = FindInScene<RoadBuilderManager>();
            BuildHudController buildHud = FindInScene<BuildHudController>();

            WireEach<TopHeaderHudController>(hud =>
            {
                Wire(hud, "simulation", simulation);
                Wire(hud, "buildingSystem", buildings);
            });

            WireEach<TreeForestrySystem>(component =>
            {
                Wire(component, "resourceStock", stock);
                Wire(component, "familyManager", families);
                Wire(component, "gridManager", grid);
            });

            WireEach<BuildingSystem>(component =>
            {
                Wire(component, "resourceStock", stock);
                Wire(component, "forestrySystem", forestry);
            });

            WireEach<ToolModeController>(component =>
            {
                Wire(component, "gridManager", grid);
                Wire(component, "previewSystem", FindInScene<PreviewSystem>());
                Wire(component, "inputHandler", FindInScene<InputHandler>());
                Wire(component, "buildingSystem", buildings);
                Wire(component, "placementHistory", FindInScene<PlacementHistory>());
                Wire(component, "roadBuilderManager", roadManager);
            });

            WireEach<RoadBuilderManager>(component => Wire(component, "_roadBuilderSystem", roadSystem));
            WireEach<RoadBuilderSystem>(component => Wire(component, "gridManager", grid));
            WireEach<BuildHudController>(component => Wire(component, "toolModeController", tools));
            WireEach<BuildingMenuItemButton>(component => Wire(component, "buildHudController", buildHud));
            WireEach<MusicLayer>(component => Wire(component, "dayNightCycle", dayNight));
            WireEach<PopulationCharacterSpawner>(component => Wire(component, "familyManager", families));
            WireEach<InputHandler>(component => Wire(component, "gridManager", grid));
        }

        private static void AssignDefaultData(ClimateSystem climate, ResourceStock stock, FamilyManager families)
        {
            if (!AssetDatabase.IsValidFolder(ConfigFolder))
            {
                Debug.LogWarning($"[Reclaim] Pasta {ConfigFolder} não encontrada; dados padrão não aplicados.");
                return;
            }

            ClimateProfile profile = LoadFirst<ClimateProfile>();
            Wire(climate, "profile", profile);

            SerializedObject stockObject = new SerializedObject(stock);
            SerializedProperty definitions = stockObject.FindProperty("definitions");
            if (definitions.arraySize == 0)
            {
                string[] guids = AssetDatabase.FindAssets("t:ResourceDefinition", new[] { ConfigFolder });
                for (int i = 0; i < guids.Length; i++)
                {
                    definitions.InsertArrayElementAtIndex(i);
                    definitions.GetArrayElementAtIndex(i).objectReferenceValue =
                        AssetDatabase.LoadAssetAtPath<ResourceDefinition>(AssetDatabase.GUIDToAssetPath(guids[i]));
                }

                stockObject.ApplyModifiedProperties();
            }

            SerializedObject familiesObject = new SerializedObject(families);
            SerializedProperty startingFamilies = familiesObject.FindProperty("startingFamilies");
            FamilyPresetData preset = LoadFirst<FamilyPresetData>();
            if (startingFamilies.arraySize == 0 && preset != null)
            {
                startingFamilies.InsertArrayElementAtIndex(0);
                SerializedProperty group = startingFamilies.GetArrayElementAtIndex(0);
                group.FindPropertyRelative("preset").objectReferenceValue = preset;
                group.FindPropertyRelative("count").intValue = 7;
                familiesObject.ApplyModifiedProperties();
            }
        }

        private static GameObject FindOrCreateRoot()
        {
            ColonySimulation existing = FindInScene<ColonySimulation>();
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject root = new GameObject(SimulationObjectName);
            Undo.RegisterCreatedObjectUndo(root, "Criar Simulação");
            return root;
        }

        private static T Ensure<T>(GameObject root) where T : Component
        {
            T component = root.GetComponent<T>();
            return component != null ? component : Undo.AddComponent<T>(root);
        }

        private static void Wire(Object target, string propertyName, Object value)
        {
            if (target == null || value == null)
            {
                return;
            }

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"[Reclaim] Campo '{propertyName}' não existe em {target.GetType().Name}.", target);
                return;
            }

            if (property.objectReferenceValue == null)
            {
                property.objectReferenceValue = value;
                serialized.ApplyModifiedProperties();
            }
        }

        private static void WireEach<T>(System.Action<T> wire) where T : Object
        {
            T[] components = Object.FindObjectsByType<T>(FindObjectsInactive.Include);
            for (int i = 0; i < components.Length; i++)
            {
                wire(components[i]);
            }
        }

        private static T FindInScene<T>() where T : Object
        {
            return Object.FindAnyObjectByType<T>(FindObjectsInactive.Include);
        }

        private static T LoadFirst<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { ConfigFolder });
            return guids.Length > 0 ? AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0])) : null;
        }
    }
}
