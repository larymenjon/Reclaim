using System;
using UnityEngine;

namespace Reclaim.UI
{
    [Serializable]
    public class MapTooltipData
    {
        [SerializeField] private string mapName;
        [SerializeField] private Sprite previewImage;

        [TextArea(2, 6)]
        [SerializeField] private string description;

        public string MapName => mapName;
        public Sprite PreviewImage => previewImage;
        public string Description => description;
    }
}
