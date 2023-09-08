using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable
namespace Assets.Src.World
{
    [Serializable]
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
        public UnityEngine.Vector2 RelativePos
        {
            protected set;
            get;
        }

        public float temperature, moisture, rockiness;

        public BiomeId? BiomeId { get; protected set; }
        public Biome? Biome => BiomeList.Get(BiomeId);

        List<WorldFeature> features = new();

        public Chunk(int worldSize, UnityEngine.Vector2 pos)
        {
            try
            {
                Pos = pos;

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
            KeyValuePair<BiomeId?, float> bestBiomeMatch = new(null, float.MaxValue);

            foreach (Biome biome in BiomeList.BIOMES.Values)
            {
                float score = biome.GetRelativeScore(this);

                if (score < bestBiomeMatch.Value)
                    bestBiomeMatch = new(biome.Id, score);
            }

            BiomeId = bestBiomeMatch.Key;
        }

        public Color32 GetMapColor()
        {
            return features.FirstOrDefault()?.MapColor ?? Biome?.GetColor(this) ?? new Color32(170, 170, 170, 255);
        }

        public void AddWorldFeature(WorldFeature feature)
        {
            features.Add(feature);
            features.OrderByDescending((feature) => feature.MapPriority);
        }

        public WorldFeature[] GetWorldFeatures()
        {
            return features.ToArray();
        }

        public Chunk[] GetAdjacentChunks()
        {
            List<Chunk> chunks = new();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    Chunk? adjChunk = World.instance!.GetChunk((int)Pos.x + x, (int)Pos.y + y);
                    if (adjChunk != null)
                        chunks.Add(adjChunk);
                }
            }

            return chunks.ToArray();
        }

        public bool IsMapBorder()
        {
            return Pos.x == 0 || Pos.y == 0 || Pos.x == World.instance!.Chunks!.GetLength(0) - 1 || Pos.y == World.instance!.Chunks!.GetLength(1) - 1;
        }

    }
}
