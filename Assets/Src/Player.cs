using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;

namespace Assets.Src
{
    [Serializable]
    public class Player : INetworkSerializable
    {

        public string Id { get; protected set; }

        public float Speed { get; protected set; }


        public Player() { } //Needed for serialization

        public Player(string id)
        {
            Id = id;

            Speed = 1f;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            //DO NOT SERIALIZE ID!!

            //I hate this and there's gotta be a better way to do it
            float speed = Speed;
            serializer.SerializeValue(ref speed);
        }
    }
}