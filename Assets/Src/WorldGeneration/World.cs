using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable
namespace Assets.Src.WorldGeneration
{
    public class World
    {

        public static World? instance;

        /// <summary>
        /// Avoid accessing this directly, use <see cref="GetChunk(int, int)"/> instead
        /// </summary>
        public Chunk[,]? Chunks
        {
            protected set;
            get;
        }

        /// <summary>
        /// Which direction is colder in the world (1 = north, -1 = south)
        /// </summary>
        public int Hemisphere
        {
            protected set;
            get;
        } = 1; //Initial value has to come after set/get protection levels

        public World(int size)
        {
            instance = this;

            //Init chunks
            Chunks = new Chunk[size, size];

            for(int x = 0; x < size; x++)
                for(int y = 0; y < size; y++)
                    Chunks[x, y] = new Chunk(size, new(x, y));
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

    }
}
