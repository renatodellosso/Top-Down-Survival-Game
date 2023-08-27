using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable
namespace Assets.Src.WorldGeneration
{
    public static class BiomeList
    {
        private static readonly Biome[] BIOMES_LIST = new Biome[]
        {
            new("forest", "Forest", 
                (chunk) => new(0, 120, 40, 255),
                0.5f, 0.6f, 0.4f
            ),
            
            new("plains", "Plains",
                (chunk) => new(40, 180, 0, 255),
                0.5f, 0.45f, 0.3f
            ),

            new("mountains", "Mountains",
                (chunk) => new(180, 180, 180, 255),
                0.4f, 0.45f, 0.9f
            ),

            new("desert", "Desert",
                (chunk) => new(230, 190, 150, 255),
                0.8f, 0.2f, 0.2f
            ),

            new("tundra", "Tundra",
                (chunk) => new(230, 240, 250, 255),
                0.2f, 0.5f, 0.2f
            ),

            new("water", "Water",
                (chunk) => new(0, 0, 200, 255),
                0.5f, 0.75f, 0.2f
            ),
        };

        public static readonly Dictionary<string, Biome> BIOMES = GetBiomes();

        public static Biome? Get(string id)
        {
            return BIOMES.TryGetValue(id, out Biome biome) ? biome : null;
        }

        private static Dictionary<string, Biome> GetBiomes()
        {
            Dictionary<string, Biome> biomes = new();

            foreach (Biome biome in BIOMES_LIST)
            {
                biomes.Add(biome.Id, biome);
            }

            return biomes;
        }
    }
}
