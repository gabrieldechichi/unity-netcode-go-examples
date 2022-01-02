using Runtime.Simulation;
using UnityEngine;

namespace Runtime.Server
{
    public class ServerEntityMovement : EntityMovement
    {
        private int lastProcessedSequenceNumber;
        public override void Move(MovementInput msg)
        {
            if (IsInputValid(msg))
            {
                ExecuteMove(msg);
                lastProcessedSequenceNumber = msg.SequenceNumber;
            }
        }

        private bool IsInputValid(MovementInput msg)
        {
            //TODO: Better validation.
            const float maxClientDt = 1.0f / 20.0f;
            return Mathf.Abs(msg.FrameInputX) <= maxClientDt;
        }

        public EntitySnapshot GenerateSnapshot()
        {
            return new EntitySnapshot
            {
                EntityId = EntityId,
                PositionX = transform.position.x,
                LastProcessedSequenceNumber = lastProcessedSequenceNumber
            };
        }
    }
}
