using Zenject;
using ZooWorld.UI;

namespace ZooWorld.Infrastructure
{
    public sealed class HudSceneInstaller : Installer<HudSceneRoot, HudSceneInstaller>
    {
        private readonly HudSceneRoot _root;

        public HudSceneInstaller(HudSceneRoot root)
        {
            _root = root;
        }

        public override void InstallBindings()
        {
            _root.ValidateConfiguration();
            Container.BindInstance(_root);
            Container.BindInstance(_root.Settings);
            Container.Bind<HudController>().AsSingle();
        }
    }
}
