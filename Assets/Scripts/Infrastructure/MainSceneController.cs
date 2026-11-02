using System;
using UnityEngine;

namespace ZooWorld.Infrastructure
{
    public sealed class MainSceneController : SessionSceneRoot
    {
        [SerializeField] private Camera _worldCamera;

        public Camera WorldCamera => _worldCamera;
        public bool CanSimulate => Session != null && Session.CanSimulate;

        public override void ValidateReadiness()
        {
            base.ValidateReadiness();
            if (_worldCamera == null || _worldCamera.gameObject.scene != gameObject.scene)
            {
                throw new InvalidOperationException("Main scene requires its own explicitly assigned camera.");
            }
        }
    }
}
