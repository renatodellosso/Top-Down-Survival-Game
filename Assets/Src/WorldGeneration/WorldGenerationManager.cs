using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.WorldGeneration
{
    public static class WorldGenerationManager
    {

        public static Task task;

        static int size;

        static World world;

        static Func<Chunk, Color32> getChunkColor = (chunk) => new Color32(170, 170, 170, 255);

        public static void StartGenerationAsync(int size)
        {
            WorldGenerationManager.size = size;

            task = Task.Run(GenerateWorldAsync);
        }

        static void GenerateWorldAsync()
        {
            UnityEngine.Debug.Log($"Generating world of size {size}...");

            //Init world
            world = new(size);

            UnityEngine.Debug.Log("Generating initial chunk stats...");

            getChunkColor = (chunk) => new Color32(chunk.moisture * 255, chunk.moisture * 255, chunk.rockiness * 255, 255);

            foreach(Chunk chunk in world.chunks)
                chunk.GenerateInitialStats();
        }

        public static Color32[,] GetDisplayColors()
        {
            Color32[,] colors = new Color32[size, size];

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    colors[x, y] = getChunkColor(world?.GetChunk(x, y));

            return colors;
        }

    }
}
