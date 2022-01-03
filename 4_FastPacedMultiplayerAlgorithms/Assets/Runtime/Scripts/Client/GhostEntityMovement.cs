using System.Collections.Generic;
using Runtime.Simulation;
using UnityEngine;

namespace Runtime.Client
{
    public struct MovementTimestamp
    {
        public float ServerTime;
        public float PositionX;
    }

    public class GhostEntityMovement : ExclusiveNetworkBehaviour
    {
        [SerializeField] private int framesBehind = 1;
        protected override EntityNetworkRole EnabledRole => EntityNetworkRole.Ghost;

        private List<MovementTimestamp> buffer = new List<MovementTimestamp>();

        public void ReceiveServerSnapshot(EntitySnapshot snapshot, bool enableInterpolation)
        {
            if (!enableInterpolation)
            {
                var position = transform.position;
                position.x = snapshot.PositionX;
                transform.position = position;
            }
            else
            {
                buffer.Add(new MovementTimestamp
                {
                    ServerTime = snapshot.ServerTimeMs,
                    PositionX = snapshot.PositionX
                });
            }
        }

        public void Interpolate(float serverTimeMs, float serverFrameIntervalMs)
        {
            var renderTime = serverTimeMs - (framesBehind * serverFrameIntervalMs);
            var startIndex = FindStartIndex(renderTime);
            var endIndex = startIndex + 1;

            Debug.Log($"YO1: {renderTime}, {serverTimeMs}, {serverFrameIntervalMs}, {startIndex}, {endIndex}");

            if (startIndex >= 0 && endIndex < buffer.Count)
            {
                var start = buffer[startIndex];
                var end = buffer[endIndex];
                var percent = (renderTime - start.ServerTime) / (end.ServerTime - start.ServerTime);
                var position = transform.position;
                position.x = Mathf.Lerp(start.PositionX, end.PositionX, percent);
                transform.position = position;
            }

            var removeIndex = startIndex - 1;
            if (removeIndex >= 0)
            {
                buffer.RemoveRange(0, removeIndex);
            }
        }

        private int FindStartIndex(float renderTime)
        {
            for (int i = buffer.Count - 1; i >= 0; i--)
            {
                Debug.Log($"Buffer: {buffer[i].ServerTime}, {buffer[i].PositionX}");
                if (buffer[i].ServerTime <= renderTime)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
