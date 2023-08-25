using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable
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

            getChunkColor = (chunk) => chunk != null ?
                new Color32(
                    (byte)(chunk.temperature * 255),
                    (byte)(chunk.temperature * 255),
                    (byte)(chunk.temperature * 255),
                    //(byte)(chunk.moisture * 255),
                    //(byte)(chunk.rockiness * 255),
                    255)
                : new Color32(170, 170, 170, 255);

            //foreach has unexpected behavior when used with multidimensional arrays
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    try
                    {
                        world.GetChunk(x, y)?.GenerateInitialStats();
                    }
                    catch (Exception e)
                    {
                        //Why the heck are error not logged on other threads?
                        UnityEngine.Debug.LogError($"Failed to generate chunk at {x}, {y}: {e}");
                    }
                }
            }

            NormalizeTemperatures();
        }

        public static Color32[,] GetDisplayColors()
        {
            Color32[,] colors = new Color32[size, size];

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    colors[x, y] = getChunkColor(world?.GetChunk(x, y));

            return colors;
        }

        static void NormalizeTemperatures()
        {
            //Normalize temperature
            //Temperate exceeds 0 and 1 at this point, so we need to normalize it
            UnityEngine.Debug.Log("Normalizing temperature...");
            float minTemp = float.MaxValue, maxTemp = float.MinValue;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Chunk? chunk = world.GetChunk(x, y);

                    if (chunk == null)
                        continue;

                    if (chunk.temperature < minTemp)
                        minTemp = chunk.temperature;

                    if (chunk.temperature > maxTemp)
                        maxTemp = chunk.temperature;
                }
            }

            UnityEngine.Debug.Log($"Min temp: {minTemp}, max temp: {maxTemp}");

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Chunk? chunk = world.GetChunk(x, y);

                    if (chunk == null)
                        continue;

                    chunk.temperature = (chunk.temperature - minTemp) / (maxTemp - minTemp);
                }
            }
        }

    }
}
