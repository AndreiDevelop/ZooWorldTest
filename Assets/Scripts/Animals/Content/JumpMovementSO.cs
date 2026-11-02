using System;
using UnityEngine;
using ZooWorld.Animals.Movement;

namespace ZooWorld.Animals.Content
{
    [CreateAssetMenu(menuName = "Zoo World/Movement/Jump")]
    public sealed class JumpMovementSO : MovementSO
    {
        [SerializeField, Min(0f)] private float _interval = 1.2f;
        [SerializeField, Min(0f)] private float _duration = 0.45f;
        [SerializeField, Min(0f)] private float _distance = 1.8f;
        [SerializeField, Min(0f)] private float _height = 0.7f;
        [SerializeField, Min(0f)] private float _recoveryAcceleration = 4f;

        public float Interval => _interval;
        public float Duration => _duration;
        public float Distance => _distance;
        public float Height => _height;
        public float RecoveryAcceleration => _recoveryAcceleration;

        public bool IsValid => float.IsFinite(_interval)
            && float.IsFinite(_duration)
            && float.IsFinite(_distance)
            && float.IsFinite(_height)
            && float.IsFinite(_recoveryAcceleration)
            && _duration > 0f
            && _interval >= _duration
            && _distance > 0f
            && _height >= 0f
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

            if (visual == null)
            {
                throw new ArgumentNullException(nameof(visual));
            }

            if (!IsValid)
            {
                throw new InvalidOperationException($"Jump movement settings '{name}' are invalid.");
            }

            return new JumpMovement(body, visual, this);
        }
    }
}
