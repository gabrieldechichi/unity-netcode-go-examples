namespace Runtime.Simulation
{
    public interface IServer
    {
        void Connect(IClient newClient);
    }
}
