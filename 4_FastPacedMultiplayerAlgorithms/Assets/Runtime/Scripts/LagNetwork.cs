using System.Collections.Generic;
using Runtime.Network;
using UnityEngine;

namespace Runtime.Network
{
    public class LagNetwork : MonoBehaviour
    {
        private struct NetworkMessage
        {
            public float TimestampMs;
            public object Payload;
        }
        public float LagMs = 100;

        private Queue<NetworkMessage> messages = new Queue<NetworkMessage>();

        public void Send<T>(T payload)
        {
            var networkMsg = new NetworkMessage
            {
                TimestampMs = Time.time,
                Payload = payload
            };
            messages.Enqueue(networkMsg);
        }

        public IEnumerable<T> Receive<T>()
        {
            bool CanReceiveMessage(in NetworkMessage msg)
            {
                return (Time.time - msg.TimestampMs) >= LagMs;
            }

            while (messages.Count > 0 && CanReceiveMessage(messages.Peek()))
            {
                yield return (T)messages.Dequeue().Payload;
            }
        }
    }

}
