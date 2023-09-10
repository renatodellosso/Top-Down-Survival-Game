using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;

namespace Assets.Src
{
    public class Player : INetworkSerializable
    {

        public float Speed { get; protected set; } = 1f;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            //I hate this and there's gotta be a better way to do it
            float speed = Speed;
            serializer.SerializeValue(ref speed);
        }
    }
}