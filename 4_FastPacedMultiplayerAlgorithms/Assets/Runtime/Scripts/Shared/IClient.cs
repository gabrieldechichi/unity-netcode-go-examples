using Runtime.Network;

namespace Runtime.Simulation
{
    public interface IClient
    {
        LagNetwork Network { get; }
        LagNetwork ServerNetwork { get; set; }
        string LocalEntityId { get; set; }

        void SpawnEntity(NetworkEntity prefab, string entityId, EntityNetworkRole role);
    }
}
