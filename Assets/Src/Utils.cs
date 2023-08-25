using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Src
{
    public static class Utils
    {

        private static readonly Random RANDOM = new Random();

        public static float RandFloat(float min, float max)
        {
            return ((float)RANDOM.NextDouble()) * (max - min) + min;
        }

        public static double RandDouble(double min, double max)
        {
            return RANDOM.NextDouble() * (max - min) + min;
        }

    }
}
