using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using ZooWorld.Domain.Session;
using ZooWorld.Infrastructure;

namespace ZooWorld.Tests.PlayMode
{
    public sealed class SceneFlowTests
    {
        private const string IntroPath = "Assets/Scenes/Intro.unity";
        private const string MainPath = "Assets/Scenes/Main.unity";
        private const string HudPath = "Assets/Scenes/HUD.unity";

        private SceneFlowController _flow;

        [UnityTest]
        public IEnumerator ScenesShareOneSessionAndReleaseAllSceneState()
        {
            yield return LoadIntro();
            yield return WaitUntilReady();
            var main = FindInScene<MainSceneController>(MainPath);
            var hud = FindInScene<HudSceneRoot>(HudPath);
            Assert.That(main.Session, Is.SameAs(_flow.Session));
            Assert.That(hud.Session, Is.SameAs(_flow.Session));
            Assert.That(main.Statistics, Is.SameAs(hud.Statistics));
            Assert.That(main.Statistics, Is.SameAs(_flow.Statistics));
            Assert.That(main.CanSimulate, Is.True);
            var stop = _flow.StopAsync();
            Assert.That(_flow.Session.CanSimulate, Is.False);
            while (!stop.IsCompleted)
            {
                yield return null;
            }

            Assert.That(_flow.Session.State, Is.EqualTo(SessionState.Stopped));
            Assert.That(SceneManager.GetSceneByPath(MainPath).isLoaded, Is.False);
            Assert.That(SceneManager.GetSceneByPath(HudPath).isLoaded, Is.False);
        }

        [UnityTest]
        public IEnumerator StoppingDuringLoadDoesNotLeaveLateScenesOrBecomeReady()
        {
            yield return LoadIntro();
            while (_flow.Session == null)
            {
                yield return null;
            }

            var stop = _flow.StopAsync();
            while (!stop.IsCompleted)
            {
                yield return null;
            }

            Assert.That(_flow.Session.State, Is.EqualTo(SessionState.Stopped));
            Assert.That(_flow.Session.CanSimulate, Is.False);
            Assert.That(SceneManager.GetSceneByPath(MainPath).isLoaded, Is.False);
            Assert.That(SceneManager.GetSceneByPath(HudPath).isLoaded, Is.False);
        }

        [UnityTest]
        public IEnumerator UnexpectedHudUnloadStopsTheSharedSession()
        {
            yield return LoadIntro();
            yield return WaitUntilReady();
            yield return SceneManager.UnloadSceneAsync(HudPath);
            var stop = _flow.StopAsync();
            while (!stop.IsCompleted)
            {
                yield return null;
            }

            Assert.That(_flow.Session.State, Is.EqualTo(SessionState.Stopped));
            Assert.That(SceneManager.GetSceneByPath(MainPath).isLoaded, Is.False);
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

            var intro = SceneManager.GetSceneByPath(IntroPath);
            if (intro.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(intro);
            }
        }

        private IEnumerator LoadIntro()
        {
            yield return SceneManager.LoadSceneAsync(IntroPath, LoadSceneMode.Additive);
            _flow = FindInScene<SceneFlowController>(IntroPath);
        }

        private IEnumerator WaitUntilReady()
        {
            var deadline = Time.realtimeSinceStartup + 15f;
            while (_flow.Session == null || !_flow.Session.CanSimulate)
            {
                if (_flow.Session != null)
                {
                    Assert.That(_flow.Session.State, Is.Not.EqualTo(SessionState.Failed), _flow.Session.FailureReason);
                }

                Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), "Scene readiness timed out.");
                yield return null;
            }
        }

        private static T FindInScene<T>(string path) where T : Component
        {
            foreach (var root in SceneManager.GetSceneByPath(path).GetRootGameObjects())
            {
                var component = root.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            Assert.Fail($"Scene '{path}' has no {typeof(T).Name}.");
            return null;
        }
    }
}
