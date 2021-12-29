using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Unity.Netcode.RuntimeTests
{
    public abstract class BaseMultiInstanceTest
    {
        protected GameObject m_PlayerPrefab;
        protected NetworkManager m_ServerNetworkManager;
        protected NetworkManager[] m_ClientNetworkManagers;

        protected abstract int NbClients { get; }

        protected bool m_BypassStartAndWaitForClients = false;

        [UnitySetUp]
        public virtual IEnumerator Setup()
        {
            yield return StartSomeClientsAndServerWithPlayers(true, NbClients, _ => { });
        }

        [UnityTearDown]
        public virtual IEnumerator Teardown()
        {
            // Shutdown and clean up both of our NetworkManager instances
            try
            {
                if (MultiInstanceHelpers.ClientSceneHandler != null)
                {
                    MultiInstanceHelpers.ClientSceneHandler.CanClientsLoad -= ClientSceneHandler_CanClientsLoad;
                    MultiInstanceHelpers.ClientSceneHandler.CanClientsUnload -= ClientSceneHandler_CanClientsUnload;
                }
                MultiInstanceHelpers.Destroy();
            }
            finally
            {
                if (m_PlayerPrefab != null)
                {
                    Object.Destroy(m_PlayerPrefab);
                    m_PlayerPrefab = null;
                }
            }

            // Make sure we clean up after ourselves and destroy any remaining NetworkObjects
            // before we exit our test
            var networkObjects = Object.FindObjectsOfType<NetworkObject>().ToList();
            foreach (var networkObject in networkObjects)
            {
                Object.DestroyImmediate(networkObject);
            }

            // wait for next frame so everything is destroyed, so following tests can execute from clean environment
            int nextFrameNumber = Time.frameCount + 1;
            yield return new WaitUntil(() => Time.frameCount >= nextFrameNumber);
        }

        /// <summary>
        /// Override this method to control when clients
        /// fake-load a scene.
        /// </summary>
        protected virtual bool CanClientsLoad()
        {
            return true;
        }

        /// <summary>
        /// Override this method to control when clients
        /// fake-unload a scene.
        /// </summary>
        protected virtual bool CanClientsUnload()
        {
            return true;
        }

        /// <summary>
        /// Registers the CanClientsLoad and CanClientsUnload events of the
        /// ClientSceneHandler (default is IntegrationTestSceneHandler).
        /// </summary>
        protected void RegisterSceneManagerHandler()
        {
            MultiInstanceHelpers.ClientSceneHandler.CanClientsLoad += ClientSceneHandler_CanClientsLoad;
            MultiInstanceHelpers.ClientSceneHandler.CanClientsUnload += ClientSceneHandler_CanClientsUnload;
        }

        private bool ClientSceneHandler_CanClientsUnload()
        {
            return CanClientsUnload();
        }

        private bool ClientSceneHandler_CanClientsLoad()
        {
            return CanClientsLoad();
        }

        /// <summary>
        /// Utility to spawn some clients and a server and set them up
        /// </summary>
        /// <param name="nbClients"></param>
        /// <param name="updatePlayerPrefab">Update the prefab with whatever is needed before players spawn</param>
        /// <param name="targetFrameRate">The targetFrameRate of the Unity engine to use while this multi instance test is running. Will be reset on teardown.</param>
        /// <returns></returns>
        public IEnumerator StartSomeClientsAndServerWithPlayers(bool useHost, int nbClients, Action<GameObject> updatePlayerPrefab = null, int targetFrameRate = 60)
        {
            // Make sure there are no remaining NetworkObjects from a previous test
            // before we start our new test
            var networkObjects = Object.FindObjectsOfType<NetworkObject>().ToList();
            foreach (var netObject in networkObjects)
            {
                Object.DestroyImmediate(netObject);
            }

            // Create multiple NetworkManager instances
            if (!MultiInstanceHelpers.Create(nbClients, out NetworkManager server, out NetworkManager[] clients, targetFrameRate))
            {
                Debug.LogError("Failed to create instances");
                Assert.Fail("Failed to create instances");
            }

            m_ClientNetworkManagers = clients;
            m_ServerNetworkManager = server;

            // Create playerPrefab
            m_PlayerPrefab = new GameObject("Player");
            NetworkObject networkObject = m_PlayerPrefab.AddComponent<NetworkObject>();
            /*
             * Normally we would only allow player prefabs to be set to a prefab. Not runtime created objects.
             * In order to prevent having a Resource folder full of a TON of prefabs that we have to maintain,
             * MultiInstanceHelper has a helper function that lets you mark a runtime created object to be
             * treated as a prefab by the Netcode. That's how we can get away with creating the player prefab
             * at runtime without it being treated as a SceneObject or causing other conflicts with the Netcode.
             */
            // Make it a prefab
            MultiInstanceHelpers.MakeNetworkObjectTestPrefab(networkObject);

            if (updatePlayerPrefab != null)
            {
                updatePlayerPrefab(m_PlayerPrefab); // update player prefab with whatever is needed before players are spawned
            }

            // Set the player prefab
            server.NetworkConfig.PlayerPrefab = m_PlayerPrefab;

            for (int i = 0; i < clients.Length; i++)
            {
                clients[i].NetworkConfig.PlayerPrefab = m_PlayerPrefab;
            }

            if (!m_BypassStartAndWaitForClients)
            {
                // Start the instances and pass in our SceneManagerInitialization action that is invoked immediately after host-server
                // is started and after each client is started.
                if (!MultiInstanceHelpers.Start(useHost, server, clients))
                {
                    Debug.LogError("Failed to start instances");
                    Assert.Fail("Failed to start instances");
                }

                RegisterSceneManagerHandler();

                // Wait for connection on client side
                yield return MultiInstanceHelpers.Run(MultiInstanceHelpers.WaitForClientsConnected(clients));

                // Wait for connection on server side
                yield return MultiInstanceHelpers.Run(MultiInstanceHelpers.WaitForClientsConnectedToServer(server, useHost ? nbClients + 1 : nbClients));
            }
        }
    }
}
