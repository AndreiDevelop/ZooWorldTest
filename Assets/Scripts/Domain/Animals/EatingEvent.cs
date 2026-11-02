namespace ZooWorld.Domain.Animals
{
    public sealed class EatingEvent
    {
        public EatingEvent(AnimalModel eater, AnimalModel victim)
        {
            Eater = eater;
            Victim = victim;
        }

        public AnimalModel Eater { get; }
        public AnimalModel Victim { get; }
        public long EaterLifetimeId => Eater.LifetimeId;
        public long VictimLifetimeId => Victim.LifetimeId;
    }
}
