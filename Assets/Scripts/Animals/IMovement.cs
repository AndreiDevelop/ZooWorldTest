using UnityEngine;

namespace ZooWorld.Animals
{
    public interface IMovement
    {
        void Tick(Vector3 direction, float deltaTime);
    }
}
