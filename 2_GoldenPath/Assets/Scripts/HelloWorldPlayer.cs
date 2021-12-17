using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HelloWorld
{
    public class HelloWorldPlayer : NetworkBehaviour
    {
        NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public override void OnNetworkSpawn()
        {
            Position.OnValueChanged += OnPositionChanged;
            Move();
        }

        public void Move()
        {
            if (IsClient)
            {
                Move_ServerRpc();
            }
            else
            {
                Position.Value = GetRandomPositionOnPlane();
            }
        }

        [ServerRpc(RequireOwnership = true)]
        private void Move_ServerRpc()
        {
            if (IsServer)
            {
                Move();
            }
        }

        private void OnPositionChanged(Vector3 previousValue, Vector3 newValue)
        {
            Debug.Log($"OnPositionChanged: {(IsClient ? "Client" : "Server")}");
            transform.position = newValue;
        }

        private Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(-3f, 3f));
        }
    }
}