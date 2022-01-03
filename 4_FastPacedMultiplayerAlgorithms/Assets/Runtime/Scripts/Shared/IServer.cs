namespace Runtime.Simulation
{
    public interface IServer : IWorld
    {
        void Connect(IClient newClient);

    }
}
