using System;
using UnityEngine;
using Zenject;
using ZooWorld.UI;

namespace ZooWorld.Infrastructure
{
    public sealed class HudSceneRoot : SessionSceneRoot
    {
        [SerializeField] private DeathCountersView _counters;
        [SerializeField] private HudPresentationSO _settings;
        [SerializeField] private TastyLabelView _labelPrefab;
        [SerializeField] private RectTransform _feedbackRoot;

        public DeathCountersView Counters => _counters;
        public HudPresentationSO Settings => _settings;
        public TastyLabelView LabelPrefab => _labelPrefab;
        public RectTransform FeedbackRoot => _feedbackRoot;
        public HudController Runtime { get; private set; }

        [Inject]
        public void ConstructRuntime(HudController runtime)
        {
            Runtime = runtime;
            if (isActiveAndEnabled)
            {
                Runtime.Activate();
            }
        }

        public void ValidateConfiguration()
        {
            if (_counters == null || _settings == null || _labelPrefab == null || _feedbackRoot == null
                || _counters.gameObject.scene != gameObject.scene || _feedbackRoot.gameObject.scene != gameObject.scene)
            {
                throw new InvalidOperationException("HUD requires scene-local counters/feedback root, presentation settings and a label prefab.");
            }

            _settings.Validate();
            _counters.ValidateReferences();
            _labelPrefab.ValidateReferences();
        }

        public override void ValidateReadiness()
        {
            base.ValidateReadiness();
            ValidateConfiguration();
            if (Runtime == null || !ReferenceEquals(Runtime.Statistics, Statistics))
            {
                throw new InvalidOperationException("HUD runtime must receive the shared statistics model.");
            }
        }

        private void OnEnable()
        {
            Runtime?.Activate();
        }

        private void LateUpdate()
        {
            if (Runtime != null && Runtime.ActiveFeedbackCount > 0)
            {
                Runtime.Tick(Time.deltaTime);
            }
        }

        private void OnDisable()
        {
            Runtime?.Deactivate();
        }

        public override void Release()
        {
            Runtime?.Dispose();
            Runtime = null;
            if (_counters != null)
            {
                _counters.Release();
            }

            base.Release();
        }
    }
}
