using Runtime.Simulation;

namespace Runtime.Client
{
    public class GhostEntityMovement : ExclusiveNetworkBehaviour
    {
        protected override EntityNetworkRole EnabledRole => EntityNetworkRole.Ghost;

        public void ReceiveServerSnapshot(EntitySnapshot snapshot)
        {
            var position = transform.position;
            position.x = snapshot.PositionX;
            transform.position = position;
        }
    }
}
