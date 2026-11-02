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
            Container.BindInstance(_root.Settings);
            Container.BindInstance(_root.LabelPrefab);
            Container.BindInstance(_root.FeedbackRoot);
            Container.Bind<HudController>().AsSingle();
        }
    }
}
