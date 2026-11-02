using System;
using TMPro;
using UnityEngine;
using Zenject;
using ZooWorld.Domain.Session;

namespace ZooWorld.UI
{
    public sealed class DeathCountersView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _prey;
        [SerializeField] private TMP_Text _predators;

        private DeathStatisticsModel _statistics;
        private bool _subscribed;

        public string PreyText => _prey.text;
        public string PredatorText => _predators.text;

        [Inject]
        public void Construct(DeathStatisticsModel statistics, HudPresentationSO settings)
        {
            Release();
            ValidateReferences();
            _statistics = statistics;
            _prey.fontSize = settings.CounterFontSize;
            _predators.fontSize = settings.CounterFontSize;
            Subscribe();
        }

        public void ValidateReferences()
        {
            if (_prey == null || _predators == null)
            {
                throw new InvalidOperationException("HUD counters require separate prey and predator TMP labels.");
            }
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_statistics == null || _subscribed || !isActiveAndEnabled)
            {
                return;
            }

            _statistics.Changed += Render;
            _subscribed = true;
            Render();
        }

        private void Render()
        {
            _prey.SetText("Dead prey: {0}", _statistics.DeadPrey);
            _predators.SetText("Dead predators: {0}", _statistics.DeadPredators);
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                _statistics.Changed -= Render;
                _subscribed = false;
            }
        }

        public void Release()
        {
            OnDisable();
            _statistics = null;
        }
    }
}
