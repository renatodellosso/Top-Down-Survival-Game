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
        }

    }
}
