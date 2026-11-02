using System;
using UnityEngine;
using ZooWorld.Domain.Animals;
using Random = UnityEngine.Random;

namespace ZooWorld.Animals.Content
{
    [CreateAssetMenu(menuName = "ZooWorld/Animal Definition")]
    public sealed class AnimalDefinitionSO : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private TrophicRole _role;
        [SerializeField] private Vector2 _sizeRange;
        [SerializeField] private MovementSO _movement;
        [SerializeField] private AnimalView _prefab;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public TrophicRole Role => _role;
        public Vector2 SizeRange => _sizeRange;
        public MovementSO Movement => _movement;
        public AnimalView Prefab => _prefab;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(_id) || string.IsNullOrWhiteSpace(_displayName))
            {
                throw Invalid("ID and display name are required.");
            }

            if (!Enum.IsDefined(typeof(TrophicRole), _role))
            {
                throw Invalid("Food-chain role is invalid.");
            }

            if (!float.IsFinite(_sizeRange.x) || !float.IsFinite(_sizeRange.y)
                || _sizeRange.x <= 0f || _sizeRange.y < _sizeRange.x)
            {
                throw Invalid("Size range must be finite, positive and ordered min <= max.");
            }

            if (_icon == null || _movement == null || _prefab == null)
            {
                throw Invalid("Icon, movement definition and animal prefab references are required.");
            }

            _prefab.ValidateReferences();
            _movement.Validate();
        }

        public float SampleSize()
        {
            Validate();
            return Random.Range(_sizeRange.x, _sizeRange.y);
        }

        public void ValidateForSimulation()
        {
            Validate();
            if (_role == TrophicRole.Omnivore)
            {
                throw Invalid("Omnivore contact rules and counter mapping are not defined; spawning is disabled.");
            }
        }

        private InvalidOperationException Invalid(string reason)
        {
            return new InvalidOperationException($"Animal definition '{name}' ({_id}): {reason}");
        }
    }
}
