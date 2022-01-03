using Runtime.Network;

namespace Runtime.Simulation
{
    public interface IWorld
    {
        NetworkEntity GetEntity(string id);
    }
    public interface IClient : IWorld
    {
        LagNetwork Network { get; }
        LagNetwork ServerNetwork { get; set; }
        string LocalEntityId { get; set; }

        void SpawnEntity(NetworkEntity prefab, string entityId, EntityNetworkRole role);
    }
}
