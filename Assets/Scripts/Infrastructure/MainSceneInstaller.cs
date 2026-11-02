using Zenject;
using ZooWorld.Animals;
using ZooWorld.Simulation;

namespace ZooWorld.Infrastructure
{
    public sealed class MainSceneInstaller : Installer<MainSceneController, MainSceneInstaller>
    {
        private readonly MainSceneController _root;

        public MainSceneInstaller(MainSceneController root)
        {
            _root = root;
        }

        public override void InstallBindings()
        {
            _root.ValidateConfiguration();
            Container.BindInstance(_root.Settings);
            Container.BindInstance(new WorldBounds(_root.WorldCamera));
            Container.Bind<AnimalLifetimeFactory>().AsSingle();
            Container.Bind<AnimalPool>().FromMethod(CreatePool).AsSingle();
            Container.Bind<SimulationController>().AsSingle();
        }

        private AnimalPool CreatePool(InjectContext context)
        {
            return new AnimalPool(context.Container.Resolve<AnimalLifetimeFactory>(), _root.AnimalsRoot,
                _root.Settings, context.Container.Resolve<WorldBounds>());
        }
    }
}
