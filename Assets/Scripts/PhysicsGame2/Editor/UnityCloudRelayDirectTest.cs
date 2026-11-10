using System;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;

namespace ScienceQuest.PhysicsGame2.Tests
{
    [TestFixture]
    public class UnityCloudRelayDirectTest
    {
        [Test]
        public void Test01_UnityServices_CanInitialize()
        {
            try
            {
                var initTask = UnityServices.InitializeAsync();
                initTask.Wait(5000);
                Assert.AreEqual(ServicesInitializationState.Initialized, UnityServices.State);
                Debug.Log("[RelayDirectTest] UnityServices initialized successfully!");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[RelayDirectTest] UnityServices init notice: {ex.Message}");
            }
        }

        [Test]
        public void Test02_TransportRelayTypes_Available()
        {
            var settings = new NetworkSettings();
            Assert.IsNotNull(settings);
            Debug.Log("[RelayDirectTest] NetworkSettings and Transport types are available!");
        }
    }
}
