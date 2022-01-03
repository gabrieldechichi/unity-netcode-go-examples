using Runtime.Network;

namespace Runtime.Simulation
{
    public interface IWorld
    {
        LagNetwork Network { get; }
        int UpdatesPerSecond { get; }
        NetworkEntity GetEntity(string id);
    }

    public interface IClient : IWorld
    {
        IServer Server { get; set; }
        string LocalEntityId { get; set; }

        void SpawnEntity(NetworkEntity prefab, string entityId, EntityNetworkRole role);
    }
}
