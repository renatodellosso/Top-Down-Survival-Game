using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Src.WorldGeneration
{
    public class World
    {

        public Chunk[,] Chunks;

        public World(int size)
        {
            //Init chunks
            Chunks = new Chunk[size, size];
        }

        public Chunk? GetChunk(int x, int y)
        {
            if (x < 0 || x >= Chunks.GetLength(0) || y < 0 || y >= Chunks.GetLength(1))
                return null;

            return Chunks[x, y];
        }

        public Chunk? GetChunk(Vector2 pos)
        {
            return GetChunk((int)pos.x, (int)pos.y);
        }

    }
}
