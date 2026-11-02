using System;
using UnityEngine;
using Zenject;
using ZooWorld.Domain.Animals;

namespace ZooWorld.Animals
{
    public sealed class AnimalView : MonoBehaviour
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private Rigidbody _body;

        private Vector3 _authoredScale;
        private bool _hasAuthoredScale;
        private Vector3 _visualRestPosition;
        private Quaternion _visualRestRotation;
        private SphereCollider _collisionBody;

        public AnimalModel Model { get; private set; }
        public Rigidbody Body => _body;
        public Transform Visual => _visual;
        public float CollisionRadius => _collisionBody.radius * Mathf.Max(
            Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));

        [Inject]
        public void Construct(AnimalModel model)
        {
            ValidateReferences();
            Release();
            if (!_hasAuthoredScale)
            {
                _authoredScale = transform.localScale;
                _hasAuthoredScale = true;
                _visualRestPosition = _visual.localPosition;
                _visualRestRotation = _visual.localRotation;
            }

            Model = model ?? throw new ArgumentNullException(nameof(model));
            Model.Died += HandleDeath;
            transform.localScale = _authoredScale * Model.CurrentSize;
            _visual.gameObject.SetActive(Model.IsAlive);
            _collisionBody = _body.GetComponent<SphereCollider>();
        }

        public void ValidateReferences()
        {
            if (_body == null || _visual == null || _body.gameObject != gameObject
                || _visual == transform || !_visual.IsChildOf(transform))
            {
                throw new InvalidOperationException($"Animal view '{name}' requires a root Rigidbody and child visual.");
            }
        }

        public void Release()
        {
            if (Model != null)
            {
                Model.Died -= HandleDeath;
                Model.Retire();
                Model = null;
            }

            if (_hasAuthoredScale)
            {
                transform.localScale = _authoredScale;
                if (_visual != null)
                {
                    _visual.localPosition = _visualRestPosition;
                    _visual.localRotation = _visualRestRotation;
                }
            }

            if (_body != null && !_body.isKinematic)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
            }
        }

        public void ActivatePhysics()
        {
            if (_collisionBody == null)
            {
                throw new InvalidOperationException($"Animal '{name}' requires a root SphereCollider.");
            }

            // Unity can retain the previous velocity while a Rigidbody is inactive.
            // Reset it after activation, while contacts are still disabled.
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            _body.detectCollisions = true;
            _collisionBody.enabled = true;
            _body.WakeUp();
        }

        public void Suspend()
        {
            _body.detectCollisions = false;
            if (_collisionBody != null)
            {
                _collisionBody.enabled = false;
            }

            _visual.gameObject.SetActive(false);
        }

        private void HandleDeath()
        {
            Suspend();
        }

        private void OnDestroy()
        {
            Release();
        }
    }
}
