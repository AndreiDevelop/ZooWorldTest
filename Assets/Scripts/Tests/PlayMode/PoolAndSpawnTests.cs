using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Zenject;
using ZooWorld.Animals;
using ZooWorld.Domain.Session;
using ZooWorld.Infrastructure;
using ZooWorld.Simulation;
using ZooWorld.Simulation.Content;

namespace ZooWorld.Tests.PlayMode
{
    public sealed class PoolAndSpawnTests
    {
        private SceneFlowController _flow;
        private MainSceneController _main;
        private AnimalPool _pool;
        private SimulationController _runtime;
        private SessionModel _session;
        private SimulationSO _settings;
        private Camera _camera;
        private GameObject _root;

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
            _settings = Object.Instantiate(_main.Settings);
            SetField(_settings, "_spawnInterval", Vector2.one);
            _root = new GameObject("TestAnimals");
            _camera = new GameObject("TestCamera").AddComponent<Camera>();
            _camera.enabled = false;
            _camera.orthographic = true;
            _camera.orthographicSize = 8.5f;
            _camera.aspect = 16f / 9f;
            _camera.transform.SetPositionAndRotation(Vector3.up * 24f, Quaternion.Euler(90f, 0f, 0f));
            var container = new DiContainer();
            SessionInstaller.Install(container);
            container.Bind<AnimalLifetimeFactory>().AsSingle();
            _session = container.Resolve<SessionModel>();
            var bounds = new WorldBounds(_camera);
            _pool = new AnimalPool(container.Resolve<AnimalLifetimeFactory>(), _root.transform, _settings, bounds);
            _runtime = new SimulationController(_session, _pool, _settings, bounds, _main.Obstacles);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            _runtime?.Dispose();
            Object.Destroy(_root);
            if (_camera != null)
            {
                Object.Destroy(_camera.gameObject);
            }

            Object.Destroy(_settings);
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
        public void RepeatedReuseResetsPhysicsPoseAndLifetimeAndRejectsStaleCallbacks()
        {
            ReadySession();
            var definition = _settings.GetSpecies(0);
            var first = _pool.Acquire(definition, Vector3.zero);
            var view = first.View;
            var previousId = first.Model.LifetimeId;
            for (var index = 0; index < 12; index++)
            {
                first.View.Body.linearVelocity = new Vector3(4f, 0f, 3f);
                first.View.Visual.localPosition = Vector3.up * 3f;
                first.View.Visual.localRotation = Quaternion.Euler(0f, 80f, 0f);
                Assert.That(_pool.RequestRelease(first), Is.True);
                Assert.That(first.Controller.CanSimulate, Is.False);
                Assert.That(first.Model.TryDie(), Is.False);
                _pool.Tick(0f);
                var next = _pool.Acquire(definition, Vector3.right);
                Assert.That(next.View, Is.SameAs(view));
                Assert.That(next.Model.LifetimeId, Is.GreaterThan(previousId));
                Assert.That(next.View.Body.linearVelocity, Is.EqualTo(Vector3.zero));
                Assert.That(next.View.Body.angularVelocity, Is.EqualTo(Vector3.zero));
                Assert.That(next.View.Visual.localPosition, Is.EqualTo(Vector3.zero));
                Assert.That(next.View.Visual.localRotation, Is.EqualTo(Quaternion.identity));
                Assert.That(next.View.transform.localScale.x, Is.EqualTo(next.Model.CurrentSize).Within(0.001f));
                Assert.That(next.View.Body.detectCollisions, Is.True);
                Assert.That(_pool.RequestRelease(first), Is.False);
                first.Controller.Tick(0.02f);
                Assert.That(next.View.Body.linearVelocity, Is.EqualTo(Vector3.zero));
                previousId = next.Model.LifetimeId;
                first = next;
            }

            Assert.That(_pool.CreatedCount, Is.EqualTo(1));
            Assert.That(_pool.ReusedCount, Is.EqualTo(12));
        }

        [Test]
        public void PrefabPoolsRemainSeparateAndDeathReleaseDoesNotCountAsPredation()
        {
            ReadySession();
            var frog = _pool.Acquire(_settings.GetSpecies(0), Vector3.zero);
            var snake = _pool.Acquire(_settings.GetSpecies(1), Vector3.right * 4f);
            frog.Model.TryDie();
            Assert.That(frog.View.Body.detectCollisions, Is.False);
            Assert.That(_pool.Active.Count, Is.EqualTo(2));
            _pool.Tick(0f);
            Assert.That(_pool.Active.Count, Is.EqualTo(1));
            var recycled = _pool.Acquire(_settings.GetSpecies(0), Vector3.zero);
            Assert.That(recycled.View, Is.SameAs(frog.View));
            Assert.That(recycled.View, Is.Not.SameAs(snake.View));
            Assert.That(_flow.Statistics.DeadPrey + _flow.Statistics.DeadPredators, Is.Zero);
            _session.BeginStopping();
            Assert.That(_pool.Active.Count, Is.Zero);
            Assert.That(recycled.Model.IsActive, Is.False);
        }

        [Test]
        public void SpawnCadenceStartsAfterReadinessAndStopsWithTheSession()
        {
            _runtime.Update(20f);
            Assert.That(_runtime.SpawnedCount, Is.Zero);
            ReadySession();
            _runtime.Update(0.99f);
            Assert.That(_runtime.SpawnedCount, Is.Zero);
            _runtime.Update(0.02f);
            Assert.That(_runtime.SpawnedCount, Is.EqualTo(1));
            _runtime.Update(0.99f);
            Assert.That(_runtime.SpawnedCount, Is.EqualTo(1));
            _runtime.Update(0.02f);
            Assert.That(_runtime.SpawnedCount, Is.EqualTo(2));
            _session.BeginStopping();
            _runtime.Update(20f);
            Assert.That(_runtime.SpawnedCount, Is.EqualTo(2));
            Assert.That(_pool.Active.Count, Is.Zero);
        }

        [TestCase(0.75f)]
        [TestCase(1.777778f)]
        [TestCase(2.2f)]
        public void BoundsFollowCameraAspectAndReturnLargeAnimalsInward(float aspect)
        {
            _camera.aspect = aspect;
            var bounds = new WorldBounds(_camera);
            var outside = new Vector3(_camera.orthographicSize * aspect + 3f, 0f, 0f);
            Assert.That(bounds.GetReturnDirection(outside, Vector3.right, 1f).x, Is.LessThan(0f));
            _camera.aspect = aspect * 0.5f;
            outside = new Vector3(_camera.orthographicSize * _camera.aspect + 1f, 0f, 0f);
            Assert.That(bounds.GetReturnDirection(outside, Vector3.right, 1f).x, Is.LessThan(0f));
            for (var index = 0; index < 20; index++)
            {
                var position = bounds.RandomPosition(1f);
                Assert.That(Mathf.Abs(position.x), Is.LessThan(_camera.orthographicSize * _camera.aspect - 0.99f));
                Assert.That(Mathf.Abs(position.z), Is.LessThan(_camera.orthographicSize - 0.99f));
            }
        }

        private void ReadySession()
        {
            _session.BeginLoading();
            _session.MarkMainReady();
            _session.MarkHudReady();
        }

        private static void SetField(Object target, string field, object value)
        {
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
    }
}
