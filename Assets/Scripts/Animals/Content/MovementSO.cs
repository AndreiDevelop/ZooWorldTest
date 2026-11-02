using UnityEngine;

namespace ZooWorld.Animals.Content
{
    public abstract class MovementSO : ScriptableObject
    {
        public abstract void Validate();

        public abstract IMovement Create(Rigidbody body, Transform visual);
    }
}
