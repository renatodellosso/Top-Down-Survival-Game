using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Assets.Src.WorldGeneration
{
    public class Chunk
    {

        public Vector2 pos;

        /// <summary>
        /// Percentage of the way to the world border
        /// </summary>
        public Vector2 RelativePos {
            protected set;
            get;
        }

        public float temperature, moisture, rockiness;

        public Chunk(int worldSize, Vector2 pos)
        {
            try
            {
                this.pos = pos;

                //Calculate relative pos
                float equator = worldSize / 2f;
                RelativePos = new Vector2(
                    //Distance from midpoint, divided from the distance to the border from the midpoint
                    (pos.X - equator) / equator,
                    (pos.Y - equator) / equator);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Failed to create chunk at {pos}: {e}");
            }
        }

        public void GenerateInitialStats()
        {
            temperature = Utils.RandFloat(0, 1) + RelativePos.Y;
            moisture = Utils.RandFloat(0, 1);
            rockiness = Utils.RandFloat(0, 1);
        }

    }
}
