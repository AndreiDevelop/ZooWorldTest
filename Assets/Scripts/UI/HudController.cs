using System;
using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Animals;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Session;
using Object = UnityEngine.Object;

namespace ZooWorld.UI
{
    public sealed class HudController : IDisposable
    {
        private readonly TastyLabelView _labelPrefab;
        private readonly RectTransform _feedbackRoot;
        private readonly SessionModel _session;
        private readonly DeathStatisticsModel _statistics;
        private readonly PredationService _predation;
        private readonly AnimalPool _animals;
        private readonly Camera _camera;
        private readonly HudPresentationSO _settings;
        private readonly List<FeedbackLease> _active = new List<FeedbackLease>();
        private readonly Stack<TastyLabelView> _inactive = new Stack<TastyLabelView>();
        private bool _subscribed;
        private bool _disposed;

        public HudController(HudPresentationSO settings, TastyLabelView labelPrefab, RectTransform feedbackRoot,
            SessionModel session, DeathStatisticsModel statistics, PredationService predation, AnimalPool animals, Camera camera)
        {
            _labelPrefab = labelPrefab;
            _feedbackRoot = feedbackRoot;
            _session = session;
            _statistics = statistics;
            _predation = predation;
            _animals = animals;
            _camera = camera;
            _settings = settings;
        }

        public DeathStatisticsModel Statistics => _statistics;
        public int ActiveFeedbackCount => _active.Count;
        public int CreatedLabelCount { get; private set; }
        public int PresentedMealCount { get; private set; }

        public TastyLabelView GetActiveLabel(int index)
        {
            return _active[index].Label;
        }

        public void Activate()
        {
            if (_subscribed || _disposed)
            {
                return;
            }

            _predation.EatingCommitted += HandleMeal;
            _session.StateChanged += HandleSessionState;
            _subscribed = true;
        }

        public void Deactivate()
        {
            if (_subscribed)
            {
                _predation.EatingCommitted -= HandleMeal;
                _session.StateChanged -= HandleSessionState;
                _subscribed = false;
            }

            for (var index = _active.Count - 1; index >= 0; index--)
            {
                ReturnLabel(index);
            }
        }

        private void HandleSessionState(SessionState state)
        {
            if (state == SessionState.Stopping || state == SessionState.Stopped || state == SessionState.Failed)
            {
                Deactivate();
            }
        }

        private void HandleMeal(EatingEvent meal)
        {
            if (!_session.CanSimulate || !meal.Eater.IsAlive || !meal.Eater.IsActive)
            {
                return;
            }

            foreach (var animal in _animals.Active)
            {
                if (ReferenceEquals(animal.Model, meal.Eater) && animal.View != null)
                {
                    var label = AcquireLabel();
                    var lease = new FeedbackLease(label, animal.View, meal.Eater, FindSlot(meal.Eater));
                    _active.Add(lease);
                    meal.Eater.Died += HandleTargetDeath;
                    PresentedMealCount++;
                    Render(lease);
                    return;
                }
            }
        }

        private TastyLabelView AcquireLabel()
        {
            TastyLabelView label = null;
            while (_inactive.Count > 0 && label == null)
            {
                label = _inactive.Pop();
            }

            if (label == null)
            {
                label = Object.Instantiate(_labelPrefab, _feedbackRoot);
                CreatedLabelCount++;
            }

            label.SetFontSize(_settings.FeedbackFontSize);
            label.ResetPresentation();
            label.gameObject.SetActive(true);
            return label;
        }

        private int FindSlot(AnimalModel model)
        {
            var slot = 0;
            var occupied = true;
            while (occupied)
            {
                occupied = false;
                foreach (var lease in _active)
                {
                    if (ReferenceEquals(lease.Model, model) && lease.Slot == slot)
                    {
                        occupied = true;
                        slot++;
                        break;
                    }
                }
            }

            return slot;
        }

        private void HandleTargetDeath()
        {
            RemoveInvalidTargets();
        }

        private void RemoveInvalidTargets()
        {
            for (var index = _active.Count - 1; index >= 0; index--)
            {
                if (!_active[index].IsCurrent)
                {
                    ReturnLabel(index);
                }
            }
        }

        public void Tick(float deltaTime)
        {
            if (!_subscribed || !_session.CanSimulate)
            {
                return;
            }

            for (var index = _active.Count - 1; index >= 0; index--)
            {
                var lease = _active[index];
                lease.Age += deltaTime;
                if (!lease.IsCurrent || lease.Age >= _settings.FeedbackDuration)
                {
                    ReturnLabel(index);
                }
                else
                {
                    Render(lease);
                }
            }
        }

        private void Render(FeedbackLease lease)
        {
            var point = lease.Target.transform.position - _camera.transform.up * lease.Target.CollisionRadius;
            var screen = _camera.WorldToScreenPoint(point);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_feedbackRoot, screen, null, out var local);
            var progress = lease.Age / _settings.FeedbackDuration;
            var offset = _settings.FeedbackOffset + Vector2.down * (lease.Slot * _settings.StackSpacing);
            local += offset + Vector2.up * (_settings.RiseDistance * progress);
            var halfSize = ((RectTransform)lease.Label.transform).rect.size * 0.5f;
            var area = _feedbackRoot.rect;
            local.x = Mathf.Clamp(local.x, area.xMin + halfSize.x, area.xMax - halfSize.x);
            local.y = Mathf.Clamp(local.y, area.yMin + halfSize.y, area.yMax - halfSize.y);
            lease.Label.Show(local, screen.z > 0f ? 1f - progress * progress : 0f);
        }

        private void ReturnLabel(int index)
        {
            var lease = _active[index];
            _active.RemoveAt(index);
            lease.Model.Died -= HandleTargetDeath;
            if (lease.Label == null)
            {
                return;
            }

            lease.Label.gameObject.SetActive(false);
            lease.Label.ResetPresentation();
            if (_inactive.Count < _settings.MaxInactiveLabels && !_disposed)
            {
                _inactive.Push(lease.Label);
            }
            else
            {
                Object.Destroy(lease.Label.gameObject);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Deactivate();
            foreach (var label in _inactive)
            {
                if (label != null)
                {
                    Object.Destroy(label.gameObject);
                }
            }

            _inactive.Clear();
        }
    }
}
