using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Assets.Src.WorldGeneration
{
    public class Chunk
    {

        public float temperature, moisture, rockiness;

        public void GenerateInitialStats()
        {
            temperature = UnityEngine.Random.Range(0f, 1f);
            moisture = UnityEngine.Random.Range(0f, 1f);
            rockiness = UnityEngine.Random.Range(-1f, 1f);
        }

    }
}
