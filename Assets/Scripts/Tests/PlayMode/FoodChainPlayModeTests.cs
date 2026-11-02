using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using ZooWorld.Animals;
using ZooWorld.Animals.Content;
using ZooWorld.Domain.Session;
using ZooWorld.Infrastructure;

namespace ZooWorld.Tests.PlayMode
{
    public sealed class FoodChainPlayModeTests
    {
        private readonly List<AnimalDefinitionSO> _definitions = new List<AnimalDefinitionSO>();
        private SceneFlowController _flow;
        private MainSceneController _main;
        private AnimalPool _pool;

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
            _pool = _main.Runtime.Pool;
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

            foreach (var definition in _definitions)
            {
                Object.Destroy(definition);
            }

            _definitions.Clear();
            yield return SceneManager.UnloadSceneAsync("Assets/Scenes/Intro.unity");
        }

        [UnityTest]
        public IEnumerator PhysicalMealDisablesContactsCountsOnceAndRecyclesWithStaleCallbacksRejected()
        {
            var frog = _pool.Acquire(_main.Settings.GetSpecies(0), Vector3.zero);
            var snake = _pool.Acquire(_main.Settings.GetSpecies(1), Vector3.right * 3f);
            var oldModel = frog.Model;
            var frogAdapter = frog.View.GetComponent<AnimalContactAdapter>();
            var snakeAdapter = snake.View.GetComponent<AnimalContactAdapter>();
            snake.View.Body.linearVelocity = Vector3.left * 4f;
            yield return WaitForDeath(frog);
            Assert.That(_main.Statistics.DeadPrey, Is.EqualTo(1));
            Assert.That(_main.Statistics.DeadPredators, Is.Zero);
            Assert.That(frog.View.Body.detectCollisions, Is.False);
            Assert.That(frog.View.Visual.gameObject.activeSelf, Is.False);
            Assert.That(_pool.Active.Count, Is.EqualTo(2), "The victim must not be reused inside contact dispatch.");
            Assert.That(snakeAdapter.TryResolveContact(snake.Model, frog.Model), Is.False);
            _pool.Tick(0f);
            Assert.That(_pool.Active.Count, Is.EqualTo(1));
            var reused = _pool.Acquire(_main.Settings.GetSpecies(0), Vector3.right * 8f);
            Assert.That(reused.View, Is.SameAs(frog.View));
            Assert.That(reused.Model.LifetimeId, Is.Not.EqualTo(oldModel.LifetimeId));
            Assert.That(frogAdapter.TryResolveContact(oldModel, snake.Model), Is.False);
            Assert.That(snakeAdapter.TryResolveContact(snake.Model, oldModel), Is.False);
            Assert.That(reused.Model.IsAlive, Is.True);
            Assert.That(_main.Statistics.DeadPrey, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator PreyPairReboundsWithoutFoodChainDeaths()
        {
            var first = _pool.Acquire(_main.Settings.GetSpecies(0), Vector3.left * 1.2f);
            var second = _pool.Acquire(_main.Settings.GetSpecies(0), Vector3.right * 1.2f);
            first.View.Body.linearVelocity = Vector3.right * 2f;
            second.View.Body.linearVelocity = Vector3.left * 2f;
            var step = new WaitForFixedUpdate();
            for (var index = 0; index < 40; index++)
            {
                yield return step;
            }

            Assert.That(first.Model.IsAlive && second.Model.IsAlive, Is.True);
            Assert.That(first.View.Body.linearVelocity.x, Is.LessThan(0f));
            Assert.That(second.View.Body.linearVelocity.x, Is.GreaterThan(0f));
            Assert.That(_main.Statistics.DeadPrey + _main.Statistics.DeadPredators, Is.Zero);
        }

        [UnityTest]
        public IEnumerator LargerPredatorWinsEvenWithTheHigherLifetimeId()
        {
            yield return CollidePredators(0.8f, 1.6f, false);
        }

        [UnityTest]
        public IEnumerator EqualPredatorsResolveByLowerLifetimeId()
        {
            yield return CollidePredators(1f, 1f, true);
        }

        [Test]
        public void SessionStopInvalidatesContactAdaptersWithoutCountingTeardownDeaths()
        {
            var frog = _pool.Acquire(_main.Settings.GetSpecies(0), Vector3.zero);
            var snake = _pool.Acquire(_main.Settings.GetSpecies(1), Vector3.right * 4f);
            var adapter = snake.View.GetComponent<AnimalContactAdapter>();
            _main.Session.BeginStopping();
            Assert.That(adapter.TryResolveContact(snake.Model, frog.Model), Is.False);
            Assert.That(_main.Statistics.DeadPrey + _main.Statistics.DeadPredators, Is.Zero);
            Assert.That(_pool.Active.Count, Is.Zero);
        }

        private IEnumerator CollidePredators(float firstSize, float secondSize, bool firstWins)
        {
            var first = _pool.Acquire(PredatorDefinition(firstSize), Vector3.left * 1.5f);
            var second = _pool.Acquire(PredatorDefinition(secondSize), Vector3.right * 1.5f);
            first.View.Body.linearVelocity = Vector3.right * 2f;
            second.View.Body.linearVelocity = Vector3.left * 2f;
            yield return WaitForDeath(firstWins ? second : first);
            Assert.That(first.Model.IsAlive, Is.EqualTo(firstWins));
            Assert.That(second.Model.IsAlive, Is.EqualTo(!firstWins));
            Assert.That(_main.Statistics.DeadPredators, Is.EqualTo(1));
            Assert.That(_main.Statistics.DeadPrey, Is.Zero);
            _pool.Tick(0f);
            Assert.That(_pool.Active.Count, Is.EqualTo(1));
        }

        private AnimalDefinitionSO PredatorDefinition(float size)
        {
            var definition = Object.Instantiate(_main.Settings.GetSpecies(1));
            typeof(AnimalDefinitionSO).GetField("_sizeRange", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(definition, Vector2.one * size);
            _definitions.Add(definition);
            return definition;
        }

        private static IEnumerator WaitForDeath(AnimalLease victim)
        {
            var deadline = Time.realtimeSinceStartup + 5f;
            while (victim.Model.IsAlive)
            {
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), "Expected physical food-chain contact did not resolve.");
                yield return null;
            }
        }
    }
}
