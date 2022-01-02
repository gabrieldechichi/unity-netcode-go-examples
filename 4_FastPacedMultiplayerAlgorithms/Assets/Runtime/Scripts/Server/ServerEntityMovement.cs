using Runtime.Simulation;
using UnityEngine;

namespace Runtime.Server
{
    public class ServerEntityMovement : EntityMovement
    {
        public override void Move(MovementInput msg)
        {
            if (IsInputValid(msg))
            {
                ExecuteMove(msg);
            }
        }

        private bool IsInputValid(MovementInput msg)
        {
            //TODO: Better validation.
            const float maxClientDt = 1.0f / 20.0f;
            return Mathf.Abs(msg.FrameInputX) <= maxClientDt;
        }
    }
}
