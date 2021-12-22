namespace ClientDriven.Common
{
    public class ClientBehaviour : ExclusiveNeworkBehaviour
    {
        protected override bool ShouldBeActive()
        {
            return IsClient;
        }
    }

}
