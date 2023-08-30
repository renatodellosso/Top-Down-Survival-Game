﻿using Assets.Src.WorldGeneration.WorldFeatures;
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
    public static class WorldGeneration
    {

        public static Task? task;

        static int size;

        static World? world;

        static Func<Chunk?, Color32> getChunkColor = (chunk) => new Color32(170, 170, 170, 255);

        const int SMOOTHING_ITERATIONS = 100;

        const float ROAD_CHANCE_PER_BORDER = 0.03f;

        public static void StartGenerationAsync(int size)
        {
            WorldGeneration.size = size;

            task = CancellableTask.Run("MainWorldGeneration", GenerateWorldAsync);
        }

        static void GenerateWorldAsync()
        {
            try
            {
                Utils.Log($"Generating world of size {size}...");

                //Init world
                world = new(size);

                world.GenerateInitialStats();

                //Generate chunk stats
                GenerateInitialChunkStats();
                GenerateFinalChunkStats();

                getChunkColor = (chunk) => chunk?.GetMapColor() ?? new(170, 170, 170, 255);

                DetermineBiomes();

                GenerateRoads();
            }
            catch (Exception e)
            {
                Utils.Log(e, "Failed to generate world");
            }
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

        static void GenerateFinalChunkStats()
        {
            Utils.Log("Generating final chunk stats...");

            //Temperature
            Task tempGenerationTask = CancellableTask.Run("TempGeneration", () => SmoothSingleChunkStat(SMOOTHING_ITERATIONS, (chunk) => chunk.temperature, (chunk, val) => chunk.temperature = val))
                    .ContinueWith((task) => NormalizeSingleChunkStat((chunk) => chunk.temperature, (chunk, val) => chunk.temperature = val)),
                //Moisture
                moistureGenerationTask = CancellableTask.Run("MoistureGeneration", () => SmoothSingleChunkStat(SMOOTHING_ITERATIONS, (chunk) => chunk.moisture, (chunk, val) => chunk.moisture = val))
                    .ContinueWith((task) => NormalizeSingleChunkStat((chunk) => chunk.moisture, (chunk, val) => chunk.moisture = val)),
                //Rockiness
                rockinessGenerationTask = CancellableTask.Run("RockinessGeneration", () => SmoothSingleChunkStat(SMOOTHING_ITERATIONS, (chunk) => chunk.rockiness, (chunk, val) => chunk.rockiness = val))
                    .ContinueWith((task) => NormalizeSingleChunkStat((chunk) => chunk.rockiness, (chunk, val) => chunk.rockiness = val));

            //Wait for all tasks to finish
            Utils.Log("Waiting for chunk stat generation to finish...");
            Task.WaitAll(tempGenerationTask, moistureGenerationTask, rockinessGenerationTask);

            Utils.Log("Finished generating final chunk stats");
        }

        static void SmoothSingleChunkStat(int iterations, Func<Chunk, float> getStat, Action<Chunk, float> setStat)
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

        static void GenerateRoads()
        {
            Utils.Log("Generating roads...");

            List<List<Road>> roads = new();

            //Generate start locations
            Utils.Log("Generating start locations...");
            Chunk[] potentialStarts = world!.GetBorderChunks();
            foreach (Chunk chunk in potentialStarts)
            {
                if(Utils.RandDouble() < ROAD_CHANCE_PER_BORDER)
                {
                    Utils.Log("Starting road at " + chunk.Pos);
                    Road road = new(chunk, Utils.RandDouble() < 0.25 ? Road.RoadType.Major : Road.RoadType.Minor);
                    roads.Add(new List<Road>() { road });
                }
            }

            //Generate roads
            //Iterate through and extend each road towards the map border
            Utils.Log("Extending roads...");
            for (int i = 0; i < roads.Count; i++)
            {
                try
                {
                    List<Road> road = roads[i];
                    Road lastNode;
                    Chunk lastChunk;

                    do
                    {
                        //Get the last node and chunk in the road
                        lastNode = road[^1];
                        lastChunk = lastNode.Chunk!;

                        //Get the chunk's neighbors
                        Chunk[] neighbors = lastChunk.GetAdjacentChunks();

                        List<Chunk> potentialNextChunks = new();
                        potentialNextChunks.AddRange(neighbors);
                        potentialNextChunks = potentialNextChunks.OrderByDescending((chunk) => chunk!.rockiness).ToList();

                        if (lastNode.Direction != Vector2.zero)
                        {
                            Chunk? chunk = world.GetChunk(lastChunk.Pos + lastNode.Direction);
                            if (chunk != null)
                                potentialNextChunks.Add(chunk);
                        }

                        //Remove water chunks
                        potentialNextChunks = potentialNextChunks.Where(chunk => chunk.BiomeId != BiomeId.Water).ToList();

                        //Remove chunks that are too rocky
                        if (Math.Max(Utils.RandDouble(), Utils.RandDouble()) < potentialNextChunks.Last().rockiness)
                        {
                            Utils.Log("Removing chunk that is too rocky. Rockiness: " + potentialNextChunks.Last().rockiness);
                            potentialNextChunks.RemoveAt(potentialNextChunks.Count - 1);

                            //OrderBy is not in place!
                            potentialNextChunks = potentialNextChunks.OrderByDescending(chunk => chunk!.rockiness + Utils.RandDouble() / 3).ToList();
                        }

                        //Get the next chunk
                        Chunk nextChunk = potentialNextChunks.First();

                        //If the next chunk is null, we've reached the map border
                        if (nextChunk == null)
                        {
                            Utils.Log("Reached map border");
                            break;
                        }

                        //Create a new road
                        Road newRoad = new(nextChunk, lastNode.Type, lastNode);

                        //Add the new road to the current road
                        roads[i].Add(newRoad);

                        //Update last chunk
                        lastChunk = nextChunk;
                    }
                    while (!lastChunk.IsMapBorder());
                } catch (Exception e)
                {
                    Utils.Log(e);
                }
            }

            //Add roads to chunks
            Utils.Log("Adding roads to chunks...");
            foreach(List<Road> road in roads)
            {
                foreach(Road node in road)
                {
                    node.Chunk!.AddWorldFeature(node);
                }
            }
        }

    }
}