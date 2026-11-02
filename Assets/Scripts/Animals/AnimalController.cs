using System;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Session;

namespace ZooWorld.Animals
{
    public sealed class AnimalController
    {
        private readonly SessionModel _session;

        public AnimalController(AnimalModel model, SessionModel session)
        {
            Model = model ?? throw new ArgumentNullException(nameof(model));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public AnimalModel Model { get; }
        public bool CanSimulate => _session.CanSimulate && Model.IsAlive;
    }
}
