using System;
using UnityEngine;

namespace ZooWorld.Simulation
{
    public sealed class WorldBounds
    {
        private readonly Camera _camera;
        private readonly Plane _ground = new Plane(Vector3.up, Vector3.zero);
        private Matrix4x4 _view;
        private Matrix4x4 _projection;
        private Vector2 _minimum;
        private Vector2 _maximum;
        private bool _initialized;

        public WorldBounds(Camera camera)
        {
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));
            Refresh();
        }

        public Vector3 RandomPosition(float inset)
        {
            Refresh();
            if (_minimum.x + inset >= _maximum.x - inset || _minimum.y + inset >= _maximum.y - inset)
            {
                throw new InvalidOperationException("The camera ground area is too small for the animal spawn clearance.");
            }

            return new Vector3(
                UnityEngine.Random.Range(_minimum.x + inset, _maximum.x - inset),
                0f,
                UnityEngine.Random.Range(_minimum.y + inset, _maximum.y - inset));
        }

        public Vector3 PositionAt(float horizontal, float vertical, float inset)
        {
            Refresh();
            return new Vector3(Mathf.Lerp(_minimum.x + inset, _maximum.x - inset, horizontal),
                0f, Mathf.Lerp(_minimum.y + inset, _maximum.y - inset, vertical));
        }

        public Vector3 GetReturnDirection(Vector3 position, Vector3 direction, float radius)
        {
            Refresh();
            if (position.x < _minimum.x + radius)
            {
                direction.x = Mathf.Abs(direction.x);
            }
            else if (position.x > _maximum.x - radius)
            {
                direction.x = -Mathf.Abs(direction.x);
            }

            if (position.z < _minimum.y + radius)
            {
                direction.z = Mathf.Abs(direction.z);
            }
            else if (position.z > _maximum.y - radius)
            {
                direction.z = -Mathf.Abs(direction.z);
            }

            direction.y = 0f;
            var center = new Vector3((_minimum.x + _maximum.x) * 0.5f, 0f,
                (_minimum.y + _maximum.y) * 0.5f);
            if ((position.x < _minimum.x + radius || position.x > _maximum.x - radius)
                && Mathf.Abs(direction.x) < Mathf.Epsilon)
            {
                direction = center - position;
            }

            if ((position.z < _minimum.y + radius || position.z > _maximum.y - radius)
                && Mathf.Abs(direction.z) < Mathf.Epsilon)
            {
                direction = center - position;
            }

            return direction.normalized;
        }

        private void Refresh()
        {
            var view = _camera.cameraToWorldMatrix;
            var projection = _camera.projectionMatrix;
            if (_initialized && view == _view && projection == _projection)
            {
                return;
            }

            _minimum = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            _maximum = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (var index = 0; index < 4; index++)
            {
                var ray = _camera.ViewportPointToRay(new Vector3(index % 2, index / 2, 0f));
                if (!_ground.Raycast(ray, out var distance))
                {
                    throw new InvalidOperationException("World camera must look down onto the simulation ground plane.");
                }

                var point = ray.GetPoint(distance);
                var planar = new Vector2(point.x, point.z);
                _minimum = Vector2.Min(_minimum, planar);
                _maximum = Vector2.Max(_maximum, planar);
            }

            _view = view;
            _projection = projection;
            _initialized = true;
        }
    }
}
