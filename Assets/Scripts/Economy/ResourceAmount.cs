using System;
using UnityEngine;

namespace Reclaim.Economy
{
    /// <summary>Par recurso e quantidade, usado em custos e recompensas.</summary>
    [Serializable]
    public struct ResourceAmount
    {
        [SerializeField] private ResourceType resourceType;
        [SerializeField] private float amount;

        public ResourceAmount(ResourceType resourceType, float amount)
        {
            this.resourceType = resourceType;
            this.amount = amount;
        }

        public ResourceType ResourceType => resourceType;
        public float Amount => amount;
    }
}
