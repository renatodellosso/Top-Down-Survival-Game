using System;
using UnityEngine;

namespace Assets.Src.WorldGeneration.WorldFeatures
{
    [Serializable]
    public class Road : LinearFeature
    {
        [Serializable]
        public class RoadType
        {
            public static readonly RoadType Dirt = new(new(139, 69, 19, 255));
            public static readonly RoadType Minor = new(new(192, 192, 192, 255));
            public static readonly RoadType Major = new(new(128, 128, 128, 255));

            public Color32 color;

            public RoadType(Color32 color)
            {
                this.color = color;
            }
        }

        public RoadType Type { get; protected set; }

        public Road(Chunk chunk, RoadType roadType, LinearFeature parentNode = null) : base(chunk, parentNode, roadType.color)
        {
            this.Type = roadType;
        }
    }
}
