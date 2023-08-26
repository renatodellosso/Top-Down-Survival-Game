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

        public static Task? task;

        static int size;

        static World? world;

        static Func<Chunk?, Color32> getChunkColor = (chunk) => new Color32(170, 170, 170, 255);

        const int SMOOTHING_ITERATIONS = 100;

        public static void StartGenerationAsync(int size)
        {
            WorldGenerationManager.size = size;

            task = Task.Run(GenerateWorldAsync);
        }

        static void GenerateWorldAsync()
        {
            Utils.Log($"Generating world of size {size}...");

            //Init world
            world = new(size);

            world.GenerateInitialStats();

            //Generate chunk stats
            GenerateInitialChunkStats();
            SmoothChunkStats(SMOOTHING_ITERATIONS);
            NormalizeChunkStats();

            DetermineBiomes();
        }

        public static Color32[,] GetDisplayColors()
        {
            Color32[,] colors = new Color32[size, size];

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    colors[y, x] = getChunkColor(world?.GetChunk(x, y));

            return colors;
        }

        static void GenerateInitialChunkStats()
        {
            Utils.Log("Generating initial chunk stats...");

            getChunkColor = (chunk) => chunk != null ?
                new Color32(
                    (byte)(chunk.temperature * 255),
                    (byte)(chunk.moisture * 255),
                    (byte)(chunk.rockiness * 255),
                    255)
                : new Color32(170, 170, 170, 255);

            //foreach has unexpected behavior when used with multidimensional arrays
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    try
                    {
                        world?.GetChunk(x, y)?.GenerateInitialStats();
                    }
                    catch (Exception e)
                    {
                        //Why the heck are error not logged on other threads?
                        Utils.Log(e, $"Failed to generate chunk at ({x}, {y})");
                    }
                }
            }
        }

        static void SmoothChunkStats(int iterations)
        {
            SmoothSingleChunkStat(iterations, (chunk) => chunk.temperature, (chunk, val) => chunk.temperature = val, 10);
            SmoothSingleChunkStat(iterations, (chunk) => chunk.moisture, (chunk, val) => chunk.moisture = val, 10);
            SmoothSingleChunkStat(iterations, (chunk) => chunk.rockiness, (chunk, val) => chunk.rockiness = val, 10);
        }

        static void SmoothSingleChunkStat(int iterations, Func<Chunk, float> getStat, Action<Chunk, float> setStat, float finalScaler = 1f)
        {
            Utils.Log("Smoothing chunk stat...");

            //Create a new 2D array to store the smoothed values
            //This is necessary because we can't modify the values of the chunks while we're iterating over them
            float[,] smoothedValues = new float[size, size];

            for (int i = 0; i < iterations; i++)
            {
                Utils.Log($"Smoothing iteration {i + 1}/{iterations}...");

                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        Chunk? chunk = world?.GetChunk(x, y);

                        if (chunk == null)
                            continue;

                        float sum = 0;
                        int count = 0;

                        //Get average of surrounding chunks
                        for (int x2 = x - 1; x2 <= x + 1; x2++)
                        {
                            for (int y2 = y - 1; y2 <= y + 1; y2++)
                            {
                                Chunk? chunk2 = world?.GetChunk(x2, y2);

                                if (chunk2 == null)
                                    continue;

                                sum += getStat(chunk2);
                                count++;
                            }
                        }

                        smoothedValues[x, y] = sum / count;
                    }
                }

                //Apply the smoothed values to the chunks
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                        setStat(world?.GetChunk(x, y)!, smoothedValues[x, y]);
                }
            }

            //Multiply the stat by the final scaler
            Utils.Log("Finalizing chunk stats...");
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                    setStat(world?.GetChunk(x, y)!, getStat(world?.GetChunk(x, y)!) * finalScaler);
            }
        }

        static void NormalizeChunkStats()
        {
            //Stats exceeds 0 and 1 at this point, so we need to normalize it
            NormalizeSingleChunkStat((chunk) => chunk.temperature, (chunk, val) => chunk.temperature = val);
            NormalizeSingleChunkStat((chunk) => chunk.moisture, (chunk, val) => chunk.moisture = val);
            NormalizeSingleChunkStat((chunk) => chunk.rockiness, (chunk, val) => chunk.rockiness = val);
        }

        static void NormalizeSingleChunkStat(Func<Chunk, float> getStat, Action<Chunk, float> setStat)
        {
            Utils.Log("Normalizing chunk stat...");
            float minStat = float.MaxValue, maxStat = float.MinValue;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Chunk? chunk = world?.GetChunk(x, y);

                    if (chunk == null)
                        continue;

                    if (getStat(chunk) < minStat)
                        minStat = getStat(chunk);

                    if (getStat(chunk) > maxStat)
                        maxStat = getStat(chunk);
                }
            }

            Utils.Log($"Min stat: {minStat}, max stat: {maxStat}");

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Chunk? chunk = world?.GetChunk(x, y);

                    if (chunk == null)
                        continue;

                    setStat(chunk, (getStat(chunk) - minStat) / (maxStat - minStat));
                }
            }
        }

        static void DetermineBiomes()
        {
            Utils.Log("Determining biomes...");

            getChunkColor = (chunk) => chunk?.Biome?.GetColor(chunk) ?? new Color32(170, 170, 170, 255);

            //Determine biomes
            for(int x = 0; x < size ; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    Chunk? chunk = world?.GetChunk(x, y);

                    if (chunk == null)
                        continue;

                    chunk.DetermineBiome();
                }
            }
        }

    }
}