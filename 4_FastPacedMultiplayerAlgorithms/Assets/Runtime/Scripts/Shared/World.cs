using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Network;
using UnityEngine;

namespace Runtime.Simulation
{
    [RequireComponent(typeof(LagNetwork))]
    public abstract class World : MonoBehaviour
    {
        [SerializeField] private LayerMask renderLayer;
        [SerializeField] private Camera worldCamera;

        [SerializeField] private int updatesPerSecond = 10;

        public int UpdatesPerSecond
        {
            get => updatesPerSecond;
            set => updatesPerSecond = value;
        }

        private LagNetwork network;
        public LagNetwork Network => network == null ? network = GetComponent<LagNetwork>() : network;
        protected Dictionary<string, NetworkEntity> entities = new Dictionary<string, NetworkEntity>();

        private void Awake()
        {
            /* enabled = false; */
            worldCamera.cullingMask |= renderLayer;
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.layer = MaskToLayer(renderLayer);
            }

            var cancellationToken = gameObject.GetCancellationTokenOnDestroy();
            RunUpdate(cancellationToken).Forget();
        }

        public NetworkEntity GetEntity(string id)
        {
            return entities.TryGetValue(id, out var entity) ? entity : null;
        }

        private async UniTask RunUpdate(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var dt = (1.0f / updatesPerSecond);
                await UniTask.Delay((int)(dt * 1000));
                UpdateWorld(dt);
            }
        }

        protected abstract void UpdateWorld(float dt);

        public void SpawnEntity(NetworkEntity prefab, string entityId, EntityNetworkRole role)
        {
            var entity = Instantiate(prefab);
            entity.EntityId = entityId;
            entity.Role = role;
            entity.gameObject.layer = MaskToLayer(renderLayer);
            entity.transform.parent = transform;

            entities.Add(entityId, entity);

            entity.OnNetworkSpawn();
        }

        private static int MaskToLayer(LayerMask mask)
        {
            return (int)Mathf.Log(mask.value, 2);
        }

    }
}
