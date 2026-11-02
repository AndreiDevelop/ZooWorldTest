using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace ZooWorld.Infrastructure
{
    public sealed class SceneScope : IDisposable
    {
        private readonly Scene _scene;
        private SessionSceneRoot _root;

        public SceneScope(Scene scene, DiContainer parent)
        {
            _scene = scene;
            Container = parent.CreateSubContainer();
        }

        public DiContainer Container { get; }

        public T FindRoot<T>() where T : SessionSceneRoot
        {
            T result = null;
            foreach (var rootObject in _scene.GetRootGameObjects())
            {
                foreach (var candidate in rootObject.GetComponentsInChildren<T>(true))
                {
                    if (result != null)
                    {
                        throw new InvalidOperationException($"Scene '{_scene.path}' has multiple {typeof(T).Name} roots.");
                    }

                    result = candidate;
                }
            }

            if (result == null)
            {
                throw new InvalidOperationException($"Scene '{_scene.path}' requires one {typeof(T).Name} root.");
            }

            _root = result;
            return result;
        }

        public void InjectHierarchy()
        {
            foreach (var rootObject in _scene.GetRootGameObjects())
            {
                Container.InjectGameObject(rootObject);
            }

            _root.ValidateReadiness();
        }

        public void Dispose()
        {
            if (_root != null)
            {
                _root.Release();
                _root = null;
            }

            Container.UnbindAll();
        }
    }
}
