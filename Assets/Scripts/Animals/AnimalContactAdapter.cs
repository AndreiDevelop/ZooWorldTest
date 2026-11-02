using System;
using UnityEngine;
using Zenject;
using ZooWorld.Domain.Animals;

namespace ZooWorld.Animals
{
    public sealed class AnimalContactAdapter : MonoBehaviour
    {
        [SerializeField] private AnimalView _view;

        private AnimalModel _model;
        private AnimalController _controller;

        [Inject]
        public void Construct(AnimalModel model, AnimalController controller)
        {
            if (_view == null || _view.gameObject != gameObject)
            {
                throw new InvalidOperationException("Animal contact adapter requires its root AnimalView reference.");
            }

            _model = model;
            _controller = controller;
        }

        public bool TryResolveContact(AnimalModel expectedSelf, AnimalModel other)
        {
            if (_model == null || _controller == null || !ReferenceEquals(expectedSelf, _model)
                || !ReferenceEquals(_view.Model, _model) || !ReferenceEquals(_controller.Model, _model))
            {
                return false;
            }

            return _controller.TryResolveContact(other);
        }

        private void OnCollisionEnter(Collision collision)
        {
            var otherBody = collision.rigidbody;
            if (otherBody != null && otherBody.TryGetComponent<AnimalView>(out var other))
            {
                TryResolveContact(_model, other.Model);
            }
        }

        private void OnDisable()
        {
            _model = null;
            _controller = null;
        }
    }
}
