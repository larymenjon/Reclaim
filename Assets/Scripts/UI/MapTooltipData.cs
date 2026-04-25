using System;
using UnityEngine;

namespace Reclaim.UI
{
    [Serializable]
    public class MapTooltipData
    {
        public string mapName;
        public Sprite previewImage;

        [TextArea(2, 6)]
        public string description;
    }
}
