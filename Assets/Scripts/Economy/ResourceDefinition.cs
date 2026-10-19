using UnityEngine;
using UnityEngine.Serialization;

namespace Reclaim.Economy
{
    /// <summary>
    /// Dados de um recurso: nome, ícone, unidade e valores iniciais.
    /// Consumíveis (comida, água) aparecem na HUD como dias de reserva.
    /// </summary>
    [CreateAssetMenu(fileName = "Recurso", menuName = "Reclaim/Economia/Definição de Recurso")]
    public class ResourceDefinition : ScriptableObject
    {
        [SerializeField] private ResourceType resourceType;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [Tooltip("Unidade curta exibida ao lado do valor quando o recurso não é consumível (ex.: UN.).")]
        [SerializeField] private string unitLabel = "UN.";
        [Tooltip("Consumíveis mostram dias de reserva em vez da quantidade.")]
        [SerializeField] private bool isConsumable;

        [Header("Valores iniciais")]
        [FormerlySerializedAs("defaultStartingAmount")]
        [SerializeField, Min(0f)] private float startingAmount;
        [FormerlySerializedAs("defaultStorageCapacity")]
        [SerializeField, Min(1f)] private float storageCapacity = 100f;

        public ResourceType ResourceType => resourceType;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public string UnitLabel => unitLabel;
        public bool IsConsumable => isConsumable;
        public float StartingAmount => startingAmount;
        public float StorageCapacity => storageCapacity;
    }
}
