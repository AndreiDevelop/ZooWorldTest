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

        public AnimalModel Model { get; private set; }
        public Rigidbody Body => _body;
        public Transform Visual => _visual;

        [Inject]
        public void Construct(AnimalModel model)
        {
            ValidateReferences();
            Release();
            if (!_hasAuthoredScale)
            {
                _authoredScale = transform.localScale;
                _hasAuthoredScale = true;
            }

            Model = model ?? throw new ArgumentNullException(nameof(model));
            Model.Died += HandleDeath;
            transform.localScale = _authoredScale * Model.CurrentSize;
            _visual.gameObject.SetActive(Model.IsAlive);
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
                Model = null;
            }

            if (_hasAuthoredScale)
            {
                transform.localScale = _authoredScale;
            }
        }

        private void HandleDeath()
        {
            _visual.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Release();
        }
    }
}
