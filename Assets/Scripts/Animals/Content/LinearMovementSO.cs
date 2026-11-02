using System;
using UnityEngine;
using ZooWorld.Animals.Movement;

namespace ZooWorld.Animals.Content
{
    [CreateAssetMenu(menuName = "Zoo World/Movement/Linear")]
    public sealed class LinearMovementSO : MovementSO
    {
        [SerializeField, Min(0f)] private float _speed = 2f;
        [SerializeField, Min(0f)] private float _recoveryAcceleration = 4f;

        public float Speed => _speed;
        public float RecoveryAcceleration => _recoveryAcceleration;

        public bool IsValid => float.IsFinite(_speed)
            && _speed > 0f
            && float.IsFinite(_recoveryAcceleration)
            && _recoveryAcceleration > 0f;

        public override void Validate()
        {
            if (!IsValid)
            {
                throw new InvalidOperationException($"Movement settings '{name}' are invalid.");
            }
        }

        public override IMovement Create(Rigidbody body, Transform visual)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            if (!IsValid)
            {
                throw new InvalidOperationException($"Linear movement settings '{name}' are invalid.");
            }

            return new LinearMovement(body, _speed, _recoveryAcceleration);
        }
    }
}
