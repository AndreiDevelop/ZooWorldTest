using System;
using ZooWorld.Domain.Session;

namespace ZooWorld.Domain.Animals
{
    public sealed class PredationService
    {
        private readonly DeathStatisticsModel _statistics;

        public PredationService(DeathStatisticsModel statistics)
        {
            _statistics = statistics ?? throw new ArgumentNullException(nameof(statistics));
        }

        public event Action<EatingEvent> EatingCommitted;

        public bool TryResolve(AnimalModel first, AnimalModel second, out EatingEvent outcome)
        {
            outcome = null;
            if (!CanResolve(first, second))
            {
                return false;
            }

            var eater = SelectEater(first, second);
            var victim = ReferenceEquals(eater, first) ? second : first;
            if (!victim.TryCommitDeath())
            {
                return false;
            }

            if (!_statistics.CommitDeath(victim))
            {
                throw new InvalidOperationException("A committed animal death must enter statistics exactly once.");
            }

            // Commit both authoritative changes before any observer can re-enter contact resolution.
            outcome = new EatingEvent(eater, victim);
            victim.PublishDeath();
            _statistics.PublishChange();
            EatingCommitted?.Invoke(outcome);
            return true;
        }

        private bool CanResolve(AnimalModel first, AnimalModel second)
        {
            if (first == null || second == null || ReferenceEquals(first, second)
                || first.LifetimeId == second.LifetimeId)
            {
                return false;
            }

            if (!first.IsAlive || !second.IsAlive || !first.IsActive || !second.IsActive
                || _statistics.HasRecordedDeath(first.LifetimeId) || _statistics.HasRecordedDeath(second.LifetimeId))
            {
                return false;
            }

            if (first.Role == TrophicRole.Omnivore || second.Role == TrophicRole.Omnivore)
            {
                return false;
            }

            return first.Role == TrophicRole.Predator || second.Role == TrophicRole.Predator;
        }

        private static AnimalModel SelectEater(AnimalModel first, AnimalModel second)
        {
            if (first.Role != second.Role)
            {
                return first.Role == TrophicRole.Predator ? first : second;
            }

            if (first.CurrentSize != second.CurrentSize)
            {
                return first.CurrentSize > second.CurrentSize ? first : second;
            }

            return first.LifetimeId < second.LifetimeId ? first : second;
        }
    }
}
