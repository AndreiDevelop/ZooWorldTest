using System;
using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Animals.Content;
using ZooWorld.Infrastructure;
using ZooWorld.Simulation;
using ZooWorld.Simulation.Content;
using Object = UnityEngine.Object;

namespace ZooWorld.Animals
{
    public sealed class AnimalPool : IDisposable
    {
        private readonly AnimalLifetimeFactory _factory;
        private readonly Transform _root;
        private readonly SimulationSO _settings;
        private readonly WorldBounds _bounds;
        private readonly Dictionary<AnimalView, Stack<AnimalView>> _inactive = new Dictionary<AnimalView, Stack<AnimalView>>();
        private readonly Dictionary<AnimalView, AnimalLease> _leases = new Dictionary<AnimalView, AnimalLease>();
        private readonly List<AnimalLease> _active = new List<AnimalLease>();
        private readonly IReadOnlyList<AnimalLease> _readOnlyActive;
        private bool _disposed;

        public AnimalPool(AnimalLifetimeFactory factory, Transform root, SimulationSO settings, WorldBounds bounds)
        {
            _factory = factory;
            _root = root;
            _settings = settings;
            _bounds = bounds;
            _readOnlyActive = _active.AsReadOnly();
        }

        public IReadOnlyList<AnimalLease> Active => _readOnlyActive;
        public int CreatedCount { get; private set; }
        public int ReusedCount { get; private set; }

        public AnimalLease Acquire(AnimalDefinitionSO definition, Vector3 position)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AnimalPool));
            }

            definition.ValidateForSimulation();
            if (!_inactive.TryGetValue(definition.Prefab, out var stack))
            {
                stack = new Stack<AnimalView>();
                _inactive.Add(definition.Prefab, stack);
            }

            AnimalView view = null;
            while (stack.Count > 0 && view == null)
            {
                view = stack.Pop();
            }

            if (view == null)
            {
                view = Object.Instantiate(definition.Prefab, _root);
                CreatedCount++;
            }
            else
            {
                ReusedCount++;
            }

            try
            {
                view.Suspend();
                view.transform.SetPositionAndRotation(position, Quaternion.identity);
                view.Body.position = position;
                view.Body.rotation = Quaternion.identity;
                var controller = _factory.Initialize(definition, view);
                var movement = definition.Movement.Create(view.Body, view.Visual);
                controller.ConfigureMovement(movement, view, _bounds, _settings);
                var lease = new AnimalLease(view, controller, definition);
                view.name = $"{definition.DisplayName} [{lease.Model.LifetimeId}]";
                view.gameObject.SetActive(true);
                view.ActivatePhysics();
                _leases.Add(view, lease);
                _active.Add(lease);
                return lease;
            }
            catch
            {
                view.Release();
                Object.Destroy(view.gameObject);
                throw;
            }
        }

        public bool RequestRelease(AnimalLease lease)
        {
            if (_disposed || lease == null || lease.View == null || lease.PendingRelease
                || !_leases.TryGetValue(lease.View, out var current) || !ReferenceEquals(current, lease)
                || !ReferenceEquals(lease.View.Model, lease.Model))
            {
                return false;
            }

            lease.PendingRelease = true;
            lease.Model.Retire();
            lease.View.Suspend();
            lease.View.gameObject.SetActive(false);
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (_disposed)
            {
                return;
            }

            for (var index = _active.Count - 1; index >= 0; index--)
            {
                var lease = _active[index];
                if (lease.View == null)
                {
                    lease.Model.Retire();
                    _leases.Remove(lease.View);
                    _active.RemoveAt(index);
                    continue;
                }

                if (!lease.PendingRelease && !lease.Model.IsAlive)
                {
                    RequestRelease(lease);
                }

                if (lease.PendingRelease)
                {
                    ReturnToStorage(index, lease);
                }
                else
                {
                    lease.Controller.Tick(deltaTime);
                }
            }
        }

        public bool HasClearance(Vector3 position, float radius)
        {
            foreach (var lease in _active)
            {
                if (lease.PendingRelease)
                {
                    continue;
                }

                var separation = radius + lease.View.CollisionRadius + _settings.SpawnSeparation;
                if ((lease.View.Body.position - position).sqrMagnitude < separation * separation)
                {
                    return false;
                }
            }

            return true;
        }

        private void ReturnToStorage(int index, AnimalLease lease)
        {
            _active.RemoveAt(index);
            _leases.Remove(lease.View);
            var view = lease.View;
            view.Release();
            var stack = _inactive[lease.Definition.Prefab];
            if (stack.Count < _settings.MaxInactivePerPrefab)
            {
                stack.Push(view);
            }
            else
            {
                Object.Destroy(view.gameObject);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            foreach (var lease in _active)
            {
                lease.Model.Retire();
                if (lease.View != null)
                {
                    lease.View.Suspend();
                    lease.View.gameObject.SetActive(false);
                    lease.View.Release();
                    Object.Destroy(lease.View.gameObject);
                }
            }

            foreach (var stack in _inactive.Values)
            {
                foreach (var view in stack)
                {
                    if (view != null)
                    {
                        Object.Destroy(view.gameObject);
                    }
                }
            }

            _active.Clear();
            _leases.Clear();
            _inactive.Clear();
        }
    }
}
