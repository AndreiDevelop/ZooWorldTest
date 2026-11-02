using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using ZooWorld.Animals;
using ZooWorld.Infrastructure;

namespace ZooWorld.Tests.PlayMode
{
    public sealed class HudPlayModeTests
    {
        private SceneFlowController _flow;
        private MainSceneController _main;
        private HudSceneRoot _hud;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return LoadSession();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (_flow != null)
            {
                yield return StopSession();
            }
        }

        [Test]
        public void CountersRenderInitialAndCommittedTotalsOnceForBothVictimRoles()
        {
            Assert.That(_hud.Counters.PreyText, Is.EqualTo("Dead prey: 0"));
            Assert.That(_hud.Counters.PredatorText, Is.EqualTo("Dead predators: 0"));
            var snake = Spawn(1);
            var frog = Spawn(0);
            Assert.That(snake.Controller.TryResolveContact(frog.Model), Is.True);
            Assert.That(snake.Controller.TryResolveContact(frog.Model), Is.False);
            var rival = Spawn(1);
            Assert.That(snake.Controller.TryResolveContact(rival.Model), Is.True);
            Assert.That(_hud.Counters.PreyText, Is.EqualTo("Dead prey: 1"));
            Assert.That(_hud.Counters.PredatorText, Is.EqualTo("Dead predators: 1"));
            Assert.That(_hud.Runtime.PresentedMealCount, Is.EqualTo(2));
        }

        [Test]
        public void FeedbackFollowsEaterExpiresAndReusesTheLabel()
        {
            var snake = Spawn(1);
            snake.Controller.TryResolveContact(Spawn(0).Model);
            var runtime = _hud.Runtime;
            Assert.That(runtime.ActiveFeedbackCount, Is.EqualTo(1));
            var label = runtime.GetActiveLabel(0);
            var before = label.Position;
            snake.View.transform.position += Vector3.right * 2f;
            runtime.Tick(0f);
            Assert.That(label.Position.x, Is.GreaterThan(before.x));
            runtime.Tick(_hud.Settings.FeedbackDuration + 0.01f);
            Assert.That(runtime.ActiveFeedbackCount, Is.Zero);
            Assert.That(label.gameObject.activeSelf, Is.False);
            snake.Controller.TryResolveContact(Spawn(0).Model);
            Assert.That(runtime.GetActiveLabel(0), Is.SameAs(label));
            Assert.That(runtime.CreatedLabelCount, Is.EqualTo(1));
        }

        [Test]
        public void SimultaneousMealsStackAndEaterDeathRemovesAllOldLabelsImmediately()
        {
            var snake = Spawn(1);
            snake.Controller.TryResolveContact(Spawn(0).Model);
            snake.Controller.TryResolveContact(Spawn(0).Model);
            var runtime = _hud.Runtime;
            Assert.That(runtime.ActiveFeedbackCount, Is.EqualTo(2));
            Assert.That(runtime.GetActiveLabel(0).Position.y,
                Is.Not.EqualTo(runtime.GetActiveLabel(1).Position.y));
            snake.Model.TryDie();
            Assert.That(runtime.ActiveFeedbackCount, Is.Zero);
        }

        [Test]
        public void RecycledAnimalCannotInheritFeedbackFromItsPreviousLifetime()
        {
            var snake = Spawn(1);
            snake.Controller.TryResolveContact(Spawn(0).Model);
            var label = _hud.Runtime.GetActiveLabel(0);
            var oldModel = snake.Model;
            _main.Runtime.Pool.RequestRelease(snake);
            _main.Runtime.Pool.Tick(0f);
            var reused = Spawn(1);
            Assert.That(reused.View, Is.SameAs(snake.View));
            Assert.That(reused.Model.LifetimeId, Is.Not.EqualTo(oldModel.LifetimeId));
            _hud.Runtime.Tick(0f);
            Assert.That(_hud.Runtime.ActiveFeedbackCount, Is.Zero);
            Assert.That(label.gameObject.activeSelf, Is.False);
            reused.Controller.TryResolveContact(Spawn(0).Model);
            Assert.That(_hud.Runtime.GetActiveLabel(0), Is.SameAs(label));
        }

        [Test]
        public void HudDisableClearsFeedbackAndReenableRendersLatestStatisticsWithoutDuplicateSubscriptions()
        {
            var snake = Spawn(1);
            snake.Controller.TryResolveContact(Spawn(0).Model);
            _hud.gameObject.SetActive(false);
            Assert.That(_hud.Runtime.ActiveFeedbackCount, Is.Zero);
            snake.Controller.TryResolveContact(Spawn(0).Model);
            Assert.That(_hud.Runtime.PresentedMealCount, Is.EqualTo(1));
            _hud.gameObject.SetActive(true);
            Assert.That(_hud.Counters.PreyText, Is.EqualTo("Dead prey: 2"));
            snake.Controller.TryResolveContact(Spawn(0).Model);
            Assert.That(_hud.Runtime.ActiveFeedbackCount, Is.EqualTo(1));
            Assert.That(_hud.Runtime.PresentedMealCount, Is.EqualTo(2));
            Assert.That(_hud.Counters.PreyText, Is.EqualTo("Dead prey: 3"));
        }

        [UnityTest]
        public IEnumerator ShutdownClearsFeedbackAndNewSessionStartsWithZeroCounters()
        {
            var snake = Spawn(1);
            snake.Controller.TryResolveContact(Spawn(0).Model);
            var runtime = _hud.Runtime;
            var label = runtime.GetActiveLabel(0);
            yield return StopSession();
            Assert.That(runtime.ActiveFeedbackCount, Is.Zero);
            Assert.That(label == null, Is.True);
            yield return LoadSession();
            Assert.That(_hud.Counters.PreyText, Is.EqualTo("Dead prey: 0"));
            Assert.That(_hud.Counters.PredatorText, Is.EqualTo("Dead predators: 0"));
            Assert.That(_hud.Runtime.ActiveFeedbackCount, Is.Zero);
        }

        private AnimalLease Spawn(int species)
        {
            return _main.Runtime.Pool.Acquire(_main.Settings.GetSpecies(species), Vector3.zero);
        }

        private IEnumerator LoadSession()
        {
            yield return SceneManager.LoadSceneAsync("Assets/Scenes/Intro.unity", LoadSceneMode.Additive);
            _flow = FindRoot<SceneFlowController>("Intro");
            var deadline = Time.realtimeSinceStartup + 15f;
            while (_flow.Session == null || !_flow.Session.CanSimulate)
            {
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), _flow.Session?.FailureReason);
                yield return null;
            }

            _main = FindRoot<MainSceneController>("Main");
            _hud = FindRoot<HudSceneRoot>("HUD");
            _main.enabled = false;
        }

        private IEnumerator StopSession()
        {
            var stop = _flow.StopAsync();
            while (!stop.IsCompleted)
            {
                yield return null;
            }

            yield return SceneManager.UnloadSceneAsync("Assets/Scenes/Intro.unity");
            _flow = null;
        }

        private static T FindRoot<T>(string sceneName) where T : Component
        {
            foreach (var root in SceneManager.GetSceneByName(sceneName).GetRootGameObjects())
            {
                if (root.TryGetComponent<T>(out var component))
                {
                    return component;
                }
            }

            Assert.Fail($"Missing {typeof(T).Name} in {sceneName}.");
            return null;
        }
    }
}
