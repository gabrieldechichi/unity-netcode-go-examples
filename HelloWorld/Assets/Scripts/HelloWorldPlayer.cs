using System;
using Unity.Netcode;
using UnityEngine;

public class HelloWorldPlayer : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Move();
        }
        Position.OnValueChanged += OnPositionChange;
    }

    private void OnPositionChange(Vector3 previousValue, Vector3 newValue)
    {
        transform.position = Position.Value;
    }

    public void Move()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var randomPosition = GetRandomPositionOnPlane();
            transform.position = randomPosition;
            Position.Value = randomPosition;
        }
        else
        {
            Move_ServerRpc();
        }
    }

    //Default reliable sucks
    [ServerRpc(Delivery = RpcDelivery.Reliable)]
    void Move_ServerRpc(ServerRpcParams rpcParamgs = default)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Move();
        }
    }

    private Vector3 GetRandomPositionOnPlane()
    {
        return new Vector3(UnityEngine.Random.Range(-3f, 3f), 1f, UnityEngine.Random.Range(-3f, 3f));
    }

}