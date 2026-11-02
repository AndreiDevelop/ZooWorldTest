using System;
using UnityEngine;
using Zenject;
using ZooWorld.Simulation;
using ZooWorld.Simulation.Content;

namespace ZooWorld.Infrastructure
{
    public sealed class MainSceneController : SessionSceneRoot
    {
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Transform _animalsRoot;
        [SerializeField] private SimulationSO _simulation;

        public Camera WorldCamera => _worldCamera;
        public bool CanSimulate => Session != null && Session.CanSimulate;
        public Transform AnimalsRoot => _animalsRoot;
        public SimulationSO Settings => _simulation;
        public SimulationController Runtime { get; private set; }

        [Inject]
        public void ConstructRuntime(SimulationController runtime)
        {
            Runtime = runtime;
        }

        public void ValidateConfiguration()
        {
            if (_simulation == null || _animalsRoot == null || _animalsRoot.gameObject.scene != gameObject.scene)
            {
                throw new InvalidOperationException("Main requires explicit simulation settings and a scene-local animal root.");
            }

            _simulation.Validate();
        }

        public override void ValidateReadiness()
        {
            base.ValidateReadiness();
            ValidateConfiguration();
            if (_worldCamera == null || _worldCamera.gameObject.scene != gameObject.scene)
            {
                throw new InvalidOperationException("Main scene requires its own explicitly assigned camera.");
            }

            if (Runtime == null || !ReferenceEquals(Runtime.Session, Session))
            {
                throw new InvalidOperationException("Main runtime must receive the shared session.");
            }
        }

        private void Update()
        {
            Runtime?.Update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            Runtime?.FixedUpdate(Time.fixedDeltaTime);
        }

        public override void Release()
        {
            Runtime?.Dispose();
            Runtime = null;
            base.Release();
        }
    }
}
