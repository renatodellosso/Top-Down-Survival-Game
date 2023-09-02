using System;
using UnityEngine;

namespace Assets.Src.WorldGeneration
{
    public class Biome
    {

        public BiomeId Id { protected set; get; }
        public string Name { protected set; get; }

        public Func<Chunk, Color32> GetColor { protected set; get; }

        public float TargetTemperature { protected set; get; }
        public float TargetMoisture { protected set; get; }
        public float TargetRockiness { protected set; get; }

        public Biome(BiomeId id, string name, Func<Chunk, Color32> getColor, float targetTemperate, float targetMoisture, float targetRockiness)
        {
            Id = id;
            Name = name;
            GetColor = getColor;

            TargetTemperature = targetTemperate;
            TargetMoisture = targetMoisture;
            TargetRockiness = targetRockiness;
        }

        /// <summary>
        /// Returns a score between 0 and 1, where a lower score is a closer match
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public float GetRelativeScore(Chunk chunk)
        {
            float score = 0;

            //Get the distance from the target stats
            score += Mathf.Abs(chunk.temperature - TargetTemperature);
            score += Mathf.Abs(chunk.moisture - TargetMoisture);
            score += Mathf.Abs(chunk.rockiness - TargetRockiness);

            return score;
        }
    }
}
