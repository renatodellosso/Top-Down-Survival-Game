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

        /// <summary>
        /// Avoid ccessing this directly, use <see cref="GetChunk(int, int)"/> instead
        /// </summary>
        public Chunk[,]? chunks;

        public World(int size)
        {
            //Init chunks
            chunks = new Chunk[size, size];
        }

        public Chunk? GetChunk(int x, int y)
        {
            if (chunks == null || x < 0 || x >= chunks.GetLength(0) || y < 0 || y >= chunks.GetLength(1))
                return null;

            return chunks[x, y];
        }

        public Chunk? GetChunk(Vector2 pos)
        {
            return GetChunk((int)pos.x, (int)pos.y);
        }

    }
}
