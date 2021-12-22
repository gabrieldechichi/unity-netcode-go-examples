namespace ClientDriven.Common
{
    public class ServerBehaviour : ExclusiveNeworkBehaviour
    {
        protected override bool ShouldBeActive()
        {
            return IsServer;
        }
    }

}
