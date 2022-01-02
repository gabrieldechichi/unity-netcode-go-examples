using System.Collections.Generic;

namespace Runtime.Simulation
{
    public struct MovementInput
    {
        public string EntityId;
        public int SequenceNumber;
        public float XInput;
    }

    public struct EntitySnapshots
    {
        public List<EntitySnapshot> Snapshots;
    }

    public struct EntitySnapshot
    {
        public string EntityId { get; internal set; }
    }

}
