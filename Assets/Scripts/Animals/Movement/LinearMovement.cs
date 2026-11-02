using UnityEngine;

namespace ZooWorld.Animals.Movement
{
    public sealed class LinearMovement : IMovement
    {
        private readonly PlanarVelocityMotor _motor;
        private readonly float _speed;

        public LinearMovement(Rigidbody body, float speed, float recoveryAcceleration)
        {
            _motor = new PlanarVelocityMotor(body, recoveryAcceleration);
            _speed = speed;
        }

        public void Tick(Vector3 direction, float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            direction.y = 0f;
            var targetVelocity = direction.normalized * _speed;
            _motor.Apply(targetVelocity, deltaTime);
        }
    }
}
