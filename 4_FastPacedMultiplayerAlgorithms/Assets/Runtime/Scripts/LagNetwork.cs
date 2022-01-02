using System.Collections.Generic;
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

        private static float CurrentTimeMs => Time.time * 1000;

        public void Send<T>(T payload)
        {
            var networkMsg = new NetworkMessage
            {
                TimestampMs = CurrentTimeMs,
                Payload = payload
            };
            messages.Enqueue(networkMsg);
        }

        public IEnumerable<T> Receive<T>()
        {
            bool CanReceiveMessage(in NetworkMessage msg)
            {
                return (CurrentTimeMs - msg.TimestampMs) >= LagMs;
            }

            while (messages.Count > 0 && CanReceiveMessage(messages.Peek()))
            {
                yield return (T)messages.Dequeue().Payload;
            }
        }
    }

}
