using System;
using Zenject;
using ZooWorld.Animals;
using ZooWorld.Animals.Content;
using ZooWorld.Domain.Animals;

namespace ZooWorld.Infrastructure
{
    public sealed class AnimalLifetimeFactory
    {
        private readonly DiContainer _container;
        private readonly AnimalIdentitySequence _identities;

        public AnimalLifetimeFactory(DiContainer container, AnimalIdentitySequence identities)
        {
            _container = container;
            _identities = identities;
        }

        public AnimalController Initialize(AnimalDefinitionSO definition, AnimalView inactiveView)
        {
            if (definition == null || inactiveView == null)
            {
                throw new ArgumentNullException("Animal definition and inactive view are required.");
            }

            if (inactiveView.gameObject.activeSelf)
            {
                throw new InvalidOperationException("Initialize an animal before activating its view.");
            }

            definition.ValidateForSimulation();
            var model = new AnimalModel(_identities.Next(), definition.Id, definition.Role, definition.SampleSize());
            var lifetime = _container.CreateSubContainer();
            lifetime.BindInstance(model);
            lifetime.Bind<AnimalController>().AsSingle();
            var controller = lifetime.Resolve<AnimalController>();
            lifetime.Inject(inactiveView);
            return controller;
        }
    }
}
