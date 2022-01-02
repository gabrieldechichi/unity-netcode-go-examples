using Runtime.Simulation;
using UnityEngine;

namespace Runtime.Client
{
    public class ClientEntityMovement : EntityMovement
    {
        public override void Move(MovementInput inputMessage)
        {
            ExecuteMove(inputMessage);
        }

        public void ReceiveServerSnapshot(EntitySnapshot snapshot)
        {
            var position = transform.position;
            position.x = snapshot.PositionX;
            transform.position = position;
        }

        public MovementInput BuildInputMessage(float dt)
        {
            var movementInput = Input.GetAxisRaw("Horizontal");
            return new MovementInput
            {
                EntityId = EntityId,
                FrameInputX = movementInput * dt
            };
        }
    }
}
