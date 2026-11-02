using System;
using UnityEngine;

namespace ZooWorld.Simulation
{
    public sealed class WorldObstacles : MonoBehaviour
    {
        [SerializeField] private Collider[] _colliders;

        public int Count => _colliders.Length;

        public Collider GetCollider(int index)
        {
            return _colliders[index];
        }

        public void Validate()
        {
            if (_colliders == null || _colliders.Length == 0)
            {
                throw new InvalidOperationException("Main obstacle map requires explicit static wall colliders.");
            }

            foreach (var collider in _colliders)
            {
                if (collider == null || collider.gameObject.scene != gameObject.scene
                    || !collider.enabled || collider.isTrigger || collider.attachedRigidbody != null)
                {
                    throw new InvalidOperationException("Main walls must have enabled, scene-local static solid colliders.");
                }
            }
        }

        public bool HasClearance(Vector3 center, float radius)
        {
            foreach (var collider in _colliders)
            {
                if (collider != null && collider.enabled && collider.gameObject.activeInHierarchy
                    && (collider.ClosestPoint(center) - center).sqrMagnitude < radius * radius)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
