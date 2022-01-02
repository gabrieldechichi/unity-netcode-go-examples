using System.Collections.Generic;
using Runtime.Network;
using UnityEngine;

namespace Runtime.Simulation
{
    [RequireComponent(typeof(LagNetwork))]
    public class World : MonoBehaviour
    {
        [SerializeField] private LayerMask renderLayer;
        [SerializeField] private Camera worldCamera;

        private LagNetwork network;
        public LagNetwork Network => network == null ? network = GetComponent<LagNetwork>() : network;
        protected Dictionary<string, NetworkEntity> entities = new Dictionary<string, NetworkEntity>();

        private void Awake()
        {
            worldCamera.cullingMask = renderLayer;
        }

        public void SpawnEntity(NetworkEntity prefab, string entityId, EntityNetworkRole role)
        {
            var entity = Instantiate(prefab);
            entity.EntityId = entityId;
            entity.Role = role;
            entity.gameObject.layer = (int)Mathf.Log(renderLayer.value, 2);
            entity.transform.parent = transform;

            entities.Add(entityId, entity);
        }

    }
}
