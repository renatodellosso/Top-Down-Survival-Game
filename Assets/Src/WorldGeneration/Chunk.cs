using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Assets.Src.WorldGeneration
{
    public class Chunk
    {

        public Vector2 Pos
        {
            protected set;
            get;
        }

        /// <summary>
        /// Percentage of the way to the world border
        /// </summary>
        public Vector2 RelativePos {
            protected set;
            get;
        }

        public float temperature, moisture, rockiness;

        protected string biomeId = "";
        public Biome? Biome => BiomeList.Get(biomeId);

        public Chunk(int worldSize, Vector2 pos)
        {
            try
            {
                this.Pos = pos;

                //Calculate relative pos
                float equator = worldSize / 2f;
                RelativePos = new Vector2(
                    //Distance from midpoint, divided from the distance to the border from the midpoint
                    (pos.X - equator) / equator,
                    (pos.Y - equator) / equator);
            }
            catch (Exception e)
            {
                Utils.Log(e, $"Failed to create chunk at {pos}");
            }
        }

        public void GenerateInitialStats()
        {
            temperature = Utils.RandFloat(0, 1) + World.instance!.Hemisphere * RelativePos.Y;
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

    }
}
