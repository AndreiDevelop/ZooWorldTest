using System;
using System.Collections.Generic;
using ZooWorld.Domain.Animals;

namespace ZooWorld.Domain.Session
{
    public sealed class DeathStatisticsModel
    {
        private readonly HashSet<long> _recordedLifetimes = new HashSet<long>();

        public event Action Changed;

        public int DeadPrey { get; private set; }
        public int DeadPredators { get; private set; }

        public bool RecordDeath(AnimalModel animal)
        {
            if (animal == null)
            {
                throw new ArgumentNullException(nameof(animal));
            }

            if (animal.Role == TrophicRole.Omnivore)
            {
                throw new InvalidOperationException("Omnivore death-counter mapping is not defined.");
            }

            if (animal.IsAlive || !_recordedLifetimes.Add(animal.LifetimeId))
            {
                return false;
            }

            if (animal.Role == TrophicRole.Prey)
            {
                DeadPrey++;
            }
            else
            {
                DeadPredators++;
            }

            Changed?.Invoke();
            return true;
        }
    }
}
