using Reclaim.Survival.Events;
using Reclaim.Survival.Families;
using Reclaim.Survival.Resources;
using Reclaim.Survival.Systems;
using Reclaim.Survival.UI;
using UnityEngine;

namespace Reclaim.Survival.Managers
{
    /// <summary>
    /// Auto-creates and connects survival systems at runtime if none exist in the scene.
    /// This removes the need for manual inspector wiring during early production.
    /// </summary>
    public static class SurvivalRuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureSurvivalSystems()
        {
            if (Object.FindFirstObjectByType<GameManager>() != null)
            {
                return;
            }

            GameObject root = new GameObject("SurvivalSystems");
            root.AddComponent<TimeSystem>();
            root.AddComponent<FamilyManager>();
            root.AddComponent<ResourceManager>();
            root.AddComponent<NeedsSystem>();
            root.AddComponent<MoraleSystem>();
            root.AddComponent<EventManager>();
            root.AddComponent<GameManager>();
            root.AddComponent<EventUIBridge>();
        }
    }
}
