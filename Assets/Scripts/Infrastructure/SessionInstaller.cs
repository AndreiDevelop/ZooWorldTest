using Zenject;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Session;

namespace ZooWorld.Infrastructure
{
    public sealed class SessionInstaller : Installer<SessionInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<SessionModel>().AsSingle();
            Container.Bind<DeathStatisticsModel>().AsSingle();
            Container.Bind<PredationService>().AsSingle();
            Container.Bind<AnimalIdentitySequence>().AsSingle();
        }
    }
}
