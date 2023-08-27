using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Src
{
    public static class Utils
    {

        private static readonly Random RANDOM = new();

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
            return ((float)RANDOM.NextDouble()) * (max - min) + min;
        }

        public static double RandDouble(double min, double max)
        {
            return RANDOM.NextDouble() * (max - min) + min;
        }

        public static double RandDouble()
        {
            return RANDOM.NextDouble();
        }

    }
}
