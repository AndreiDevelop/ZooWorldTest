using System;
using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Animals.Content;

namespace ZooWorld.Simulation.Content
{
    [CreateAssetMenu(menuName = "ZooWorld/Simulation")]
    public sealed class SimulationSO : ScriptableObject
    {
        [SerializeField] private AnimalDefinitionSO[] _animals;
        [SerializeField] private Vector2 _spawnInterval;
        [SerializeField] private Vector2 _turnInterval;
        [SerializeField] private float _spawnPadding;
        [SerializeField] private float _spawnSeparation;
        [SerializeField] private int _spawnAttempts;
        [SerializeField] private int _maxInactivePerPrefab;

        public IReadOnlyList<AnimalDefinitionSO> Animals => Array.AsReadOnly(_animals);
        public float MinSpawnInterval => _spawnInterval.x;
        public float MaxSpawnInterval => _spawnInterval.y;
        public float MinTurnInterval => _turnInterval.x;
        public float MaxTurnInterval => _turnInterval.y;
        public float SpawnPadding => _spawnPadding;
        public float SpawnSeparation => _spawnSeparation;
        public int SpawnAttempts => _spawnAttempts;
        public int MaxInactivePerPrefab => _maxInactivePerPrefab;
        public int SpeciesCount => _animals.Length;

        public AnimalDefinitionSO GetSpecies(int index)
        {
            return _animals[index];
        }

        public void Validate()
        {
            if (_animals == null || _animals.Length == 0)
            {
                throw new InvalidOperationException($"Simulation '{name}' requires animal definitions.");
            }

            var identifiers = new HashSet<string>();
            foreach (var animal in _animals)
            {
                if (animal == null)
                {
                    throw new InvalidOperationException($"Simulation '{name}' contains a missing species reference.");
                }

                animal.ValidateForSimulation();
                if (!identifiers.Add(animal.Id))
                {
                    throw new InvalidOperationException($"Simulation '{name}' has duplicate species ID '{animal.Id}'.");
                }

                if (animal.Prefab.gameObject.activeSelf || animal.Prefab.Body.GetComponent<SphereCollider>() == null)
                {
                    throw new InvalidOperationException($"Species '{animal.Id}' needs an inactive prefab with a root SphereCollider.");
                }
            }

            if (!ValidRange(_spawnInterval) || _spawnInterval.x < 1f || _spawnInterval.y > 2f
                || !ValidRange(_turnInterval) || !float.IsFinite(_spawnPadding) || _spawnPadding < 0f
                || !float.IsFinite(_spawnSeparation) || _spawnSeparation < 0f
                || _spawnAttempts <= 0 || _maxInactivePerPrefab < 0)
            {
                throw new InvalidOperationException($"Simulation '{name}' has invalid spawn, turn or pool settings.");
            }
        }

        private static bool ValidRange(Vector2 range)
        {
            return float.IsFinite(range.x) && float.IsFinite(range.y) && range.x > 0f && range.y >= range.x;
        }
    }
}
