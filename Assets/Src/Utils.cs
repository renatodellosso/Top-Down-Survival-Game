﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Assets.Src
{
    public static class Utils
    {

        private static readonly Random Random = new();

        public static void Log(string msg)
        {
            UnityEngine.Debug.Log(msg);
        }

        public static void Log(Exception e, string note = "")
        {
            UnityEngine.Debug.LogError($"Caught error: {(note != "" ? note + " - " : "")}{e.Message} (@ {e.Source})\n{e.StackTrace}");
        }
        
        /// <summary>
        /// Generates a random float between min and max
        /// This casts the result of Random.NextDouble() to a float, so it is less efficient than RandDouble()
        /// </summary>
        public static float RandFloat(float min, float max)
        {
            return ((float)Random.NextDouble()) * (max - min) + min;
        }

        public static double RandDouble(double min, double max)
        {
            return Random.NextDouble() * (max - min) + min;
        }

        public static double RandDouble()
        {
            return Random.NextDouble();
        }

        public static int RandInt(int min, int max)
        {
            return Random.Next(min, max);
        }

        public static int RandInt(int max)
        {
            return Random.Next(max);
        }

        static readonly List<CancellationTokenSource> activeTasks = new();

        /// <summary>
        /// Calls Task.Run() with a CancellationToken that will be cancelled when the game is closed
        /// </summary>
        /// <param name="action">The action to execute asynchronously</param>
        /// <returns>The task that was created</returns>
        public static Task Async(Action action)
        {
            CancellationTokenSource source = new();
            activeTasks.Add(source);
            Task task = Task.Run(action, source.Token);

            return task;
        }

    }
}
