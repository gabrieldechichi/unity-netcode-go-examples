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

        private Queue<NetworkMessage> messages;

        public void Send<T>(T payload) where T : struct
        {
            var networkMsg = new NetworkMessage
            {
                TimestampMs = Time.time,
                Payload = payload
            };
            messages.Enqueue(networkMsg);
        }

        public IEnumerator<object> Receive()
        {
            bool CanReceiveMessage(in NetworkMessage msg)
            {
                return (Time.time - msg.TimestampMs) >= LagMs;
            }

            while (CanReceiveMessage(messages.Peek()))
            {
                yield return messages.Dequeue().Payload;
            }
        }
    }

}
