using System;
using UnityEngine;
using ZooWorld.Simulation;
using ZooWorld.Simulation.Content;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Session;

namespace ZooWorld.Animals
{
    public sealed class AnimalController
    {
        private readonly SessionModel _session;
        private IMovement _movement;
        private AnimalView _view;
        private WorldBounds _bounds;
        private SimulationSO _settings;
        private Vector3 _direction;
        private float _timeUntilTurn;

        public AnimalController(AnimalModel model, SessionModel session)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public AnimalModel Model { get; }
        public bool CanSimulate => _session.CanSimulate && Model.IsAlive && Model.IsActive;

        public void ConfigureMovement(IMovement movement, AnimalView view, WorldBounds bounds, SimulationSO settings)
        {
            _movement = movement ?? throw new ArgumentNullException(nameof(movement));
            _view = view;
            _bounds = bounds;
            _settings = settings;
            ChooseDirection();
        }

        public void Tick(float deltaTime)
        {
            if (!CanSimulate || _movement == null || deltaTime <= 0f)
            {
                return;
            }

            _timeUntilTurn -= deltaTime;
            if (_timeUntilTurn <= 0f)
            {
                ChooseDirection();
            }

            _direction = _bounds.GetReturnDirection(_view.Body.position, _direction, _view.CollisionRadius);
            _view.Visual.localRotation = Quaternion.LookRotation(_direction, Vector3.up);
            _movement.Tick(_direction, deltaTime);
        }

        private void ChooseDirection()
        {
            var angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            _direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            _timeUntilTurn = UnityEngine.Random.Range(_settings.MinTurnInterval, _settings.MaxTurnInterval);
        }
    }
}
