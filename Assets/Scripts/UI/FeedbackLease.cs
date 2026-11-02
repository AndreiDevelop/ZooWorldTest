using ZooWorld.Animals;
using ZooWorld.Domain.Animals;

namespace ZooWorld.UI
{
    public sealed class FeedbackLease
    {
        public FeedbackLease(TastyLabelView label, AnimalView target, AnimalModel model, int slot)
        {
            Label = label;
            Target = target;
            Model = model;
            Slot = slot;
        }

        public TastyLabelView Label { get; }
        public AnimalView Target { get; }
        public AnimalModel Model { get; }
        public int Slot { get; }
        public float Age { get; set; }
        public bool IsCurrent => Target != null && Model.IsAlive && Model.IsActive
            && Target.gameObject.activeInHierarchy && ReferenceEquals(Target.Model, Model);
    }
}
