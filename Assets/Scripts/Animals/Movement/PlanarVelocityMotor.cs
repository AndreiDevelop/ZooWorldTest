using UnityEngine;

namespace ZooWorld.Animals.Movement
{
    public sealed class PlanarVelocityMotor
    {
        private readonly Rigidbody _body;
        private readonly float _recoveryAcceleration;

        private Vector3 _previousCommand;

        public PlanarVelocityMotor(Rigidbody body, float recoveryAcceleration)
        {
            _body = body;
            _recoveryAcceleration = recoveryAcceleration;
        }

        public void Apply(Vector3 command, float deltaTime)
        {
            var velocity = _body.linearVelocity;
            var planarVelocity = new Vector3(velocity.x, 0f, velocity.z);

            // Remove only the previous motor contribution; collision recoil remains physical.
            var recoil = planarVelocity - _previousCommand;
            recoil = Vector3.MoveTowards(recoil, Vector3.zero, _recoveryAcceleration * deltaTime);
            var nextVelocity = command + recoil;
            _body.linearVelocity = new Vector3(nextVelocity.x, velocity.y, nextVelocity.z);
            _previousCommand = command;
        }
    }
}
