using System;
using UnityEngine;

#nullable enable
namespace Assets.Src.World.WorldFeatures
{
    [Serializable]
    public abstract class LinearFeature : WorldFeature
    {

        public LinearFeature? parentNode, childNode;

        public Vector2 Direction { get; protected set; }

        public LinearFeature(Chunk chunk, LinearFeature? parentNode = null, Color32? baseColor = null) : base(chunk, baseColor)
        {
            this.parentNode = parentNode;

            if (parentNode != null)
            {
                parentNode.childNode = this;

                Direction = Chunk?.Pos - parentNode.Chunk?.Pos ?? new(0, 0);
            }
        }
    }
}
