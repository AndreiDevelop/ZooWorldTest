using System;
using UnityEngine;
using ZooWorld.Animals;
using ZooWorld.Domain.Session;
using ZooWorld.Simulation.Content;

namespace ZooWorld.Simulation
{
    public sealed class SimulationController : IDisposable
    {
        private readonly SimulationSO _settings;
        private readonly WorldBounds _bounds;
        private float _timeUntilSpawn;
        private bool _disposed;

        public SimulationController(SessionModel session, AnimalPool pool, SimulationSO settings, WorldBounds bounds)
        {
            Session = session;
            Pool = pool;
            _settings = settings;
            _bounds = bounds;
            _timeUntilSpawn = NextInterval();
            Session.StateChanged += HandleStateChanged;
        }

        public SessionModel Session { get; }
        public AnimalPool Pool { get; }
        public int SpawnedCount { get; private set; }

        public void Update(float deltaTime)
        {
            if (_disposed || !Session.CanSimulate)
            {
                return;
            }

            _timeUntilSpawn -= deltaTime;
            if (_timeUntilSpawn > 0f)
            {
                return;
            }

            var definition = _settings.GetSpecies(UnityEngine.Random.Range(0, _settings.SpeciesCount));
            var radius = definition.Prefab.Body.GetComponent<SphereCollider>().radius
                * definition.SizeRange.y * Mathf.Max(definition.Prefab.transform.localScale.x,
                    definition.Prefab.transform.localScale.z);
            var position = _bounds.RandomPosition(radius + _settings.SpawnPadding);
            for (var attempt = 1; attempt < _settings.SpawnAttempts && !Pool.HasClearance(position, radius); attempt++)
            {
                position = _bounds.RandomPosition(radius + _settings.SpawnPadding);
            }

            Pool.Acquire(definition, position);
            SpawnedCount++;
            _timeUntilSpawn = NextInterval();
        }

        public void FixedUpdate(float deltaTime)
        {
            if (!_disposed && Session.CanSimulate)
            {
                Pool.Tick(deltaTime);
            }
        }

        private float NextInterval()
        {
            return UnityEngine.Random.Range(_settings.MinSpawnInterval, _settings.MaxSpawnInterval);
        }

        private void HandleStateChanged(SessionState state)
        {
            if (state == SessionState.Stopping || state == SessionState.Stopped || state == SessionState.Failed)
            {
                Dispose();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Session.StateChanged -= HandleStateChanged;
            Pool.Dispose();
        }
    }
}
