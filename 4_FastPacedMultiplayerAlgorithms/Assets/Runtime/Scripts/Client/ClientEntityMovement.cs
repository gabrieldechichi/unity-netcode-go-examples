using System.Collections.Generic;
using Runtime.Simulation;
using UnityEngine;

namespace Runtime.Client
{
    public class ClientEntityMovement : EntityMovement
    {
        private List<MovementInput> pendingInputs = new List<MovementInput>();
        private int nextInputSequencedNumber = 0;

        protected override EntityNetworkRole EnabledRole => EntityNetworkRole.OwningClient;

        protected override void OnNetworkSpawnInternal()
        {
            base.OnNetworkSpawnInternal();
            GetComponent<EntityMovementData>().CollisionMask = 0;
        }

        public override void Move(MovementInput inputMessage)
        {
            ExecuteMove(inputMessage);
        }

        public void ReceiveServerSnapshot(EntitySnapshot snapshot, bool isServerReconciliationEnabled)
        {
            var position = transform.position;
            position.x = snapshot.PositionX;
            transform.position = position;

            if (isServerReconciliationEnabled)
            {
                var maxIndexToRemove = FindMaxPendingInputIndexToRemove(snapshot.LastProcessedSequenceNumber);
                if (maxIndexToRemove >= 0)
                {
                    pendingInputs.RemoveRange(0, maxIndexToRemove + 1);
                    foreach (var input in pendingInputs)
                    {
                        Move(input);
                    }
                }
            }
        }

        private int FindMaxPendingInputIndexToRemove(int lastProcessedSequenceNumber)
        {
            for (int i = pendingInputs.Count - 1; i >= 0; i--)
            {
                if (pendingInputs[i].SequenceNumber <= lastProcessedSequenceNumber)
                {
                    return i;
                }
            }
            return -1;
        }

        public MovementInput BuildInputMessage(float dt, ClientInputType inputType)
        {
            var movementInput = 0.0f;
            switch (inputType)
            {
                case ClientInputType.None:
                    break;
                case ClientInputType.WASD:
                    movementInput = Input.GetKey(KeyCode.A) ? -1 : Input.GetKey(KeyCode.D) ? 1 : 0;
                    break;
                case ClientInputType.ArrowKeys:
                    movementInput = Input.GetKey(KeyCode.LeftArrow) ? -1 : Input.GetKey(KeyCode.RightArrow) ? 1 : 0;
                    break;
            }

            var inputMessage = new MovementInput
            {
                EntityId = EntityId,
                FrameInputX = movementInput * dt,
                SequenceNumber = nextInputSequencedNumber++
            };
            pendingInputs.Add(inputMessage);

            return inputMessage;
        }
    }
}
