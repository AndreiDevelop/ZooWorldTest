using ZooWorld.Animals.Content;
using ZooWorld.Domain.Animals;

namespace ZooWorld.Animals
{
    public sealed class AnimalLease
    {
        public AnimalLease(AnimalView view, AnimalController controller, AnimalDefinitionSO definition)
        {
            View = view;
            Controller = controller;
            Definition = definition;
        }

        public AnimalView View { get; }
        public AnimalController Controller { get; }
        public AnimalDefinitionSO Definition { get; }
        public AnimalModel Model => Controller.Model;
        public bool PendingRelease { get; internal set; }
    }
}
