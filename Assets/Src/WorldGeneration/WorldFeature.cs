using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable
namespace Assets.Src.WorldGeneration
{
    public abstract class WorldFeature
    {
        public Chunk? Chunk { get; protected set; }

        Color32? baseColor;
        public virtual Color32? MapColor => baseColor;
        public int MapPriority { get; protected set; } = 0;

        public WorldFeature(Chunk chunk, Color32? baseColor = null, int MapPriority = 0)
        {
            Chunk = chunk;
            this.baseColor = baseColor;
            this.MapPriority = MapPriority;
        }
    }
}
