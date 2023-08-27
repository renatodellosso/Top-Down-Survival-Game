using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable
namespace Assets.Src.WorldGeneration
{
    public class Chunk
    {

        public UnityEngine.Vector2 Pos
        {
            protected set;
            get;
        }

        /// <summary>
        /// Percentage of the way to the world border
        /// </summary>
        public UnityEngine.Vector2 RelativePos {
            protected set;
            get;
        }

        public float temperature, moisture, rockiness;

        protected string biomeId = "";
        public Biome? Biome => BiomeList.Get(biomeId);

        public Chunk(int worldSize, UnityEngine.Vector2 pos)
        {
            try
            {
                this.Pos = pos;

                //Calculate relative pos
                float equator = worldSize / 2f;
                RelativePos = new UnityEngine.Vector2(
                    //Distance from midpoint, divided from the distance to the border from the midpoint
                    (pos.x - equator) / equator,
                    (pos.y - equator) / equator);
            }
            catch (Exception e)
            {
                Utils.Log(e, $"Failed to create chunk at {pos}");
            }
        }

        public void GenerateInitialStats()
        {
            temperature = Utils.RandFloat(0, 1) + World.instance!.Hemisphere * RelativePos.y;
            moisture = Utils.RandFloat(0, 1);
            rockiness = Utils.RandFloat(0, 1);
        }

        public void DetermineBiome()
        {
            KeyValuePair<string, float> bestBiomeMatch = new("", float.MaxValue);

            foreach(Biome biome in BiomeList.BIOMES.Values)
            {
                float score = biome.GetRelativeScore(this);

                if (score < bestBiomeMatch.Value)
                    bestBiomeMatch = new(biome.Id, score);
            }

            biomeId = bestBiomeMatch.Key;
        }

        public Color32 GetMapColor()
        {
            return Biome?.GetColor(this) ?? new Color32(170, 170, 170, 255);
        }

    }
}
