using UnityEngine;
using ZooWorld.Animals;
using ZooWorld.Animals.Content;

namespace ZooWorld.Tests.EditMode
{
    public sealed class TestMovementSO : MovementSO
    {
        public override void Validate()
        {
        }

        public override IMovement Create(Rigidbody body, Transform visual)
        {
            throw new System.NotSupportedException("Movement implementation belongs to iteration 2.");
        }
    }
}
