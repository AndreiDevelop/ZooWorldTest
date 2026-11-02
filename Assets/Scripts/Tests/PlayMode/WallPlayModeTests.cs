using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using ZooWorld.Infrastructure;

namespace ZooWorld.Tests.PlayMode
{
    public sealed class WallPlayModeTests
    {
        private SceneFlowController _flow;
        private MainSceneController _main;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return SceneManager.LoadSceneAsync("Assets/Scenes/Intro.unity", LoadSceneMode.Additive);
            _flow = SceneManager.GetSceneByPath("Assets/Scenes/Intro.unity").GetRootGameObjects()[0]
                .GetComponent<SceneFlowController>();
            var deadline = Time.realtimeSinceStartup + 15f;
            while (_flow.Session == null || !_flow.Session.CanSimulate)
            {
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline));
                yield return null;
            }

            foreach (var root in SceneManager.GetSceneByPath("Assets/Scenes/Main.unity").GetRootGameObjects())
            {
                if (root.TryGetComponent<MainSceneController>(out var main))
                {
                    _main = main;
                }
            }

            _main.enabled = false;
            Physics.SyncTransforms();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_flow != null)
            {
                var stop = _flow.StopAsync();
                while (!stop.IsCompleted)
                {
                    yield return null;
                }
            }

            yield return SceneManager.UnloadSceneAsync("Assets/Scenes/Intro.unity");
        }

        [Test]
        public void CrowdedSpawnFallbackNeverPlacesAnimalsInsideWalls()
        {
            Assert.That(_main.Obstacles.Count, Is.EqualTo(16));
            for (var index = 0; index < 150; index++)
            {
                _main.Runtime.Update(2f);
                var active = _main.Runtime.Pool.Active;
                var view = active[active.Count - 1].View;
                var sphere = view.GetComponent<SphereCollider>();
                Assert.That(_main.Obstacles.HasClearance(view.transform.TransformPoint(sphere.center),
                    view.CollisionRadius - 0.001f), Is.True, "Spawn must remain outside every static wall.");
            }

            Assert.That(_main.Runtime.SpawnedCount, Is.EqualTo(150));
        }

        [UnityTest]
        public IEnumerator SnakeCannotPassThroughWallOrCountItAsFood()
        {
            yield return CheckWallContact(1);
        }

        [UnityTest]
        public IEnumerator FrogJumpCannotPassThroughWallOrRaiseItsPhysicsBody()
        {
            yield return CheckWallContact(0);
        }

        private IEnumerator CheckWallContact(int species)
        {
            var wall = _main.Obstacles.GetCollider(0);
            var position = new Vector3(wall.bounds.center.x, 0f, wall.bounds.min.z - 2.5f);
            var definition = _main.Settings.GetSpecies(species);
            var lease = _main.Runtime.Pool.Acquire(definition, position);
            var movement = definition.Movement.Create(lease.View.Body, lease.View.Visual);
            var closestDistance = float.PositiveInfinity;
            var step = new WaitForFixedUpdate();
            for (var index = 0; index < 180; index++)
            {
                movement.Tick(Vector3.forward, Time.fixedDeltaTime);
                yield return step;
                var body = lease.View.Body;
                var distance = wall.bounds.min.z - body.position.z - lease.View.CollisionRadius;
                closestDistance = Mathf.Min(closestDistance, distance);
                Assert.That(distance, Is.GreaterThan(-0.08f), "Grounded animal must not cross the wall face.");
                Assert.That(body.position.y, Is.EqualTo(0f).Within(0.001f));
            }

            Assert.That(closestDistance, Is.LessThan(0.1f), "The animal must reach and physically touch the wall.");
            Assert.That(lease.Model.IsAlive, Is.True);
            Assert.That(_main.Statistics.DeadPrey + _main.Statistics.DeadPredators, Is.Zero);
        }
    }
}
