using UnityEngine;
using ZooWorld.Animals.Content;

namespace ZooWorld.Animals.Movement
{
    public sealed class JumpMovement : IMovement
    {
        private readonly PlanarVelocityMotor _motor;
        private readonly Transform _visual;
        private readonly JumpMovementSO _settings;
        private readonly Vector3 _restPosition;

        private Vector3 _jumpDirection;
        private float _timeUntilJump;
        private float _elapsedJump;
        private bool _isJumping;

        public JumpMovement(Rigidbody body, Transform visual, JumpMovementSO settings)
        {
            _motor = new PlanarVelocityMotor(body, settings.RecoveryAcceleration);
            _visual = visual;
            _settings = settings;
            _restPosition = visual.localPosition;
        }

        public void Tick(Vector3 direction, float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            _timeUntilJump -= deltaTime;
            if (!_isJumping && _timeUntilJump <= 0f)
            {
                BeginJump(direction);
            }

            var command = Vector3.zero;
            if (_isJumping)
            {
                var remainingTime = _settings.Duration - _elapsedJump;
                var movementTime = Mathf.Min(deltaTime, remainingTime);
                var speed = _settings.Distance / _settings.Duration;
                command = _jumpDirection * (speed * movementTime / deltaTime);
                AdvanceVisual(deltaTime);
            }

            _motor.Apply(command, deltaTime);
        }

        private void BeginJump(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            _jumpDirection = direction.normalized;
            _elapsedJump = 0f;
            _timeUntilJump = _settings.Interval;
            _isJumping = true;
        }

        private void AdvanceVisual(float deltaTime)
        {
            _elapsedJump = Mathf.Min(_elapsedJump + deltaTime, _settings.Duration);
            var progress = _elapsedJump / _settings.Duration;
            var height = 4f * _settings.Height * progress * (1f - progress) / _visual.parent.lossyScale.y;
            _visual.localPosition = _restPosition + Vector3.up * height;

            if (_elapsedJump >= _settings.Duration)
            {
                _isJumping = false;
                _visual.localPosition = _restPosition;
            }
        }
    }
}
