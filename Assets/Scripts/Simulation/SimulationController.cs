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
        private readonly WorldObstacles _obstacles;
        private float _timeUntilSpawn;
        private bool _disposed;

        public SimulationController(SessionModel session, AnimalPool pool, SimulationSO settings, WorldBounds bounds,
            WorldObstacles obstacles)
        {
            Session = session;
            Pool = pool;
            _settings = settings;
            _bounds = bounds;
            _obstacles = obstacles;
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
            var sphere = definition.Prefab.Body.GetComponent<SphereCollider>();
            var radius = sphere.radius * definition.SizeRange.y
                * Mathf.Max(definition.Prefab.transform.localScale.x, definition.Prefab.transform.localScale.z);
            var centerOffset = Vector3.Scale(sphere.center,
                definition.Prefab.transform.localScale * definition.SizeRange.y);
            var position = FindSpawnPosition(radius, centerOffset);

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

        private Vector3 FindSpawnPosition(float radius, Vector3 centerOffset)
        {
            var fallback = Vector3.zero;
            var found = false;
            var inset = radius + _settings.SpawnPadding;
            for (var attempt = 0; attempt < _settings.SpawnAttempts; attempt++)
            {
                var candidate = _bounds.RandomPosition(inset);
                if (!_obstacles.HasClearance(candidate + centerOffset, radius))
                {
                    continue;
                }

                fallback = candidate;
                found = true;
                if (Pool.HasClearance(candidate, radius))
                {
                    return candidate;
                }
            }

            if (found)
            {
                return fallback;
            }

            // A bounded scan avoids dropping a spawn when random attempts all hit walls.
            var divisions = _settings.SpawnAttempts;
            for (var row = 0; row < divisions; row++)
            {
                for (var column = 0; column < divisions; column++)
                {
                    var candidate = _bounds.PositionAt((column + 0.5f) / divisions,
                        (row + 0.5f) / divisions, inset);
                    if (_obstacles.HasClearance(candidate + centerOffset, radius))
                    {
                        return candidate;
                    }
                }
            }

            throw new InvalidOperationException("Main has no wall-free spawn location for the selected animal.");
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
