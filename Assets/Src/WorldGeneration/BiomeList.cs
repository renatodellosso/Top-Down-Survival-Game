using System.Collections.Generic;

#nullable enable
namespace Assets.Src.WorldGeneration
{
    public enum BiomeId
    {
        Forest,
        Plains,
        Mountains,
        Desert,
        Tundra,
        Water
    }

    public static class BiomeList
    {

        private static readonly Biome[] BIOMES_LIST = new Biome[]
        {
            new(BiomeId.Forest, "Forest",
                (chunk) => new(0, 120, 40, 255),
                0.5f, 0.6f, 0.4f
            ),

            new(BiomeId.Plains, "Plains",
                (chunk) => new(40, 180, 0, 255),
                0.5f, 0.45f, 0.3f
            ),

            new(BiomeId.Mountains, "Mountains",
                (chunk) => new(180, 180, 180, 255),
                0.4f, 0.45f, 0.9f
            ),

            new(BiomeId.Desert, "Desert",
                (chunk) => new(230, 190, 150, 255),
                0.8f, 0.2f, 0.2f
            ),

            new(BiomeId.Tundra, "Tundra",
                (chunk) => new(230, 240, 250, 255),
                0.2f, 0.5f, 0.2f
            ),

            new(BiomeId.Water, "Water",
                (chunk) => new(0, 0, 200, 255),
                0.5f, 0.75f, 0.2f
            ),
        };

        public static readonly Dictionary<BiomeId, Biome> BIOMES = GetBiomes();

        public static Biome? Get(BiomeId? id)
        {
            return id != null && BIOMES.TryGetValue(id.Value, out Biome biome) ? biome : null;
        }

        private static Dictionary<BiomeId, Biome> GetBiomes()
        {
            Dictionary<BiomeId, Biome> biomes = new();

            foreach (Biome biome in BIOMES_LIST)
            {
                biomes.Add(biome.Id, biome);
            }

            return biomes;
        }
    }
}
