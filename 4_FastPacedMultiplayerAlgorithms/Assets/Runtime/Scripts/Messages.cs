using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Simulation
{
    public struct MovementInput
    {
        public string EntityId;
        public float FrameInputX;
    }

    public struct EntitySnapshots
    {
        public List<EntitySnapshot> Snapshots;
    }

    public struct EntitySnapshot
    {
        public string EntityId;
        public float PositionX;
    }
}
