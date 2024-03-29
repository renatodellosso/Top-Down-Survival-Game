﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable
namespace Assets.Src.World
{
    [Serializable]
    public class World
    {

        public static World? instance;
        readonly Chunk?[,]? chunksInternal;

        /// <summary>
        /// Avoid accessing this directly, use <see cref="GetChunk(int, int)"/> instead
        /// </summary>
        public Chunk?[,]? Chunks
        {
            get => chunksInternal;
        }

        /// <summary>
        /// Which direction is colder in the world (1 = north, -1 = south)
        /// </summary>
        public int Hemisphere
        {
            protected set;
            get;
        } = 1; //Initial value has to come after set/get protection levels

        public int Size
        {
            protected set;
            get;
        }

        public World(int size)
        {
            instance = this;

            Size = size;

            //Init chunks
            chunksInternal = new Chunk[size, size];

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    chunksInternal[x, y] = new Chunk(size, new(x, y));
        }

        public Chunk? GetChunk(int x, int y)
        {
            if (Chunks == null || x < 0 || x >= Chunks.GetLength(0) || y < 0 || y >= Chunks.GetLength(1))
                return null;

            return Chunks[x, y];
        }

        public Chunk? GetChunk(Vector2 pos)
        {
            return GetChunk((int)pos.x, (int)pos.y);
        }

        public void GenerateInitialStats()
        {
            Utils.Log("Generating initial world stats...");

            Hemisphere = Utils.RandDouble() < .5 ? 1 : -1;

            //Log initial stats
            Utils.Log($"Initial World Stats:" +
                $"\nHemisphere: {Hemisphere}");
        }

        public Chunk[] GetBorderChunks()
        {
            HashSet<Chunk> chunks = new();

            if (Chunks == null)
                return chunks.ToArray();

            for (int x = 0; x < Chunks.GetLength(0); x++)
            {
                chunks.Add(Chunks[x, 0]!);
                chunks.Add(Chunks[x, Chunks.GetLength(1) - 1]!);
            }

            for (int y = 1; y < Chunks.GetLength(1) - 1; y++)
            {
                chunks.Add(Chunks[0, y]!);
                chunks.Add(Chunks[Chunks.GetLength(0) - 1, y]!);
            }

            return chunks.ToArray();
        }

    }
}
