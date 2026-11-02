using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using ZooWorld.Domain.Session;

namespace ZooWorld.Infrastructure
{
    public sealed class SceneFlowController : MonoBehaviour
    {
        [SerializeField] private SceneFlowSO _configuration;

        private CancellationTokenSource _cancellation;
        private DiContainer _sessionContainer;
        private SceneScope _mainScope;
        private SceneScope _hudScope;
        private Scene _mainScene;
        private Scene _hudScene;
        private Task _loading;
        private Task _shutdown;
        private bool _quitting;

        public SessionModel Session { get; private set; }
        public DeathStatisticsModel Statistics { get; private set; }

        private IEnumerator Start()
        {
            _cancellation = new CancellationTokenSource();
            _sessionContainer = new DiContainer();
            SessionInstaller.Install(_sessionContainer);
            Session = _sessionContainer.Resolve<SessionModel>();
            Statistics = _sessionContainer.Resolve<DeathStatisticsModel>();
            SceneManager.sceneUnloaded += HandleSceneUnloaded;
            _loading = LoadAsync(_cancellation.Token);
            while (!_loading.IsCompleted)
            {
                yield return null;
            }
        }

        private async Task LoadAsync(CancellationToken cancellation)
        {
            try
            {
                if (_configuration == null)
                {
                    throw new InvalidOperationException("Intro requires a SceneFlowSO configuration.");
                }

                _configuration.Validate();
                Session.BeginLoading();
                await LoadSceneAsync(_configuration.MainScenePath, cancellation);
                _mainScene = SceneManager.GetSceneByPath(_configuration.MainScenePath);
                _mainScope = new SceneScope(_mainScene, _sessionContainer);
                var main = _mainScope.FindRoot<MainSceneController>();
                if (main.WorldCamera == null)
                {
                    throw new InvalidOperationException("Main scene requires an explicitly assigned camera.");
                }

                _sessionContainer.BindInstance(main.WorldCamera);
                MainSceneInstaller.Install(_mainScope.Container, main);
                _mainScope.InjectHierarchy();
                _sessionContainer.BindInstance(main.Runtime.Pool);
                Session.MarkMainReady();

                await LoadSceneAsync(_configuration.HudScenePath, cancellation);
                _hudScene = SceneManager.GetSceneByPath(_configuration.HudScenePath);
                _hudScope = new SceneScope(_hudScene, _sessionContainer);
                var hud = _hudScope.FindRoot<HudSceneRoot>();
                HudSceneInstaller.Install(_hudScope.Container, hud);
                _hudScope.InjectHierarchy();
                cancellation.ThrowIfCancellationRequested();
                SceneManager.SetActiveScene(_mainScene);
                Session.MarkHudReady();
            }
            catch (OperationCanceledException)
            {
                Session.BeginStopping();
            }
            catch (Exception exception)
            {
                Session.Fail(exception.Message);
                Debug.LogError($"ZooWorld scene initialization failed: {exception}", this);
            }

            if (!Session.CanSimulate)
            {
                await ReleaseScenesAsync();
            }
        }

        private static async Task LoadSceneAsync(string path, CancellationToken cancellation)
        {
            cancellation.ThrowIfCancellationRequested();
            if (SceneManager.GetSceneByPath(path).isLoaded)
            {
                throw new InvalidOperationException($"Scene '{path}' is already loaded outside this session.");
            }

            var operation = SceneManager.LoadSceneAsync(path, LoadSceneMode.Additive);
            await WaitAsync(operation);
            if (cancellation.IsCancellationRequested)
            {
                // Unity loads cannot be cancelled; clean up their result before completing cancellation.
                var loaded = SceneManager.GetSceneByPath(path);
                if (loaded.isLoaded)
                {
                    await WaitAsync(SceneManager.UnloadSceneAsync(loaded));
                }

                cancellation.ThrowIfCancellationRequested();
            }
        }

        public Task StopAsync()
        {
            if (_shutdown == null)
            {
                _shutdown = ShutdownAsync();
            }

            return _shutdown;
        }

        private async Task ShutdownAsync()
        {
            if (Session == null)
            {
                return;
            }

            _cancellation.Cancel();
            Session.BeginStopping();
            try
            {
                if (_loading != null)
                {
                    await _loading;
                }

                await ReleaseScenesAsync();
                Session.MarkStopped();
            }
            catch (Exception exception)
            {
                Session.Fail(exception.Message);
                Debug.LogError($"ZooWorld scene shutdown failed: {exception}");
            }
            finally
            {
                SceneManager.sceneUnloaded -= HandleSceneUnloaded;
                _sessionContainer.UnbindAll();
                _cancellation.Dispose();
                _cancellation = null;
            }
        }

        private async Task ReleaseScenesAsync()
        {
            _hudScope?.Dispose();
            _hudScope = null;
            _mainScope?.Dispose();
            _mainScope = null;
            if (_quitting || !Application.isPlaying)
            {
                return;
            }

            if (_hudScene.IsValid() && _hudScene.isLoaded)
            {
                await WaitAsync(SceneManager.UnloadSceneAsync(_hudScene));
            }

            if (_mainScene.IsValid() && _mainScene.isLoaded)
            {
                await WaitAsync(SceneManager.UnloadSceneAsync(_mainScene));
            }
        }

        private static async Task WaitAsync(AsyncOperation operation)
        {
            if (operation == null)
            {
                throw new InvalidOperationException("Unity did not start the requested scene operation.");
            }

            while (!operation.isDone)
            {
                await Task.Yield();
            }
        }

        private void OnDisable()
        {
            StopAsync();
        }

        private void HandleSceneUnloaded(Scene scene)
        {
            if (Session != null && Session.CanSimulate
                && (scene == _mainScene || scene == _hudScene))
            {
                StopAsync();
            }
        }

        private void OnApplicationQuit()
        {
            _quitting = true;
        }
    }
}
