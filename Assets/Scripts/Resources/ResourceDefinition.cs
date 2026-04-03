using UnityEngine;

namespace Reclaim.Survival.Resources
{
    [CreateAssetMenu(fileName = "ResourceDefinition", menuName = "Reclaim/Survival/Resource Definition")]
    public class ResourceDefinition : ScriptableObject
    {
        [SerializeField] private ResourceType resourceType;
        [SerializeField] private string displayName;
        [SerializeField] private float defaultStartingAmount = 0f;
        [SerializeField] private float defaultStorageCapacity = 100f;

        public ResourceType ResourceType => resourceType;
        public string DisplayName => displayName;
        public float DefaultStartingAmount => defaultStartingAmount;
        public float DefaultStorageCapacity => defaultStorageCapacity;
    }
}
