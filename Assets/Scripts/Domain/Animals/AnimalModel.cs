using System;

namespace ZooWorld.Domain.Animals
{
    public sealed class AnimalModel
    {
        public AnimalModel(long lifetimeId, string speciesId, TrophicRole role, float currentSize)
        {
            if (lifetimeId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lifetimeId));
            }

            if (string.IsNullOrWhiteSpace(speciesId))
            {
                throw new ArgumentException("Species ID is required.", nameof(speciesId));
            }

            if (!Enum.IsDefined(typeof(TrophicRole), role))
            {
                throw new ArgumentOutOfRangeException(nameof(role));
            }

            if (float.IsNaN(currentSize) || float.IsInfinity(currentSize) || currentSize <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(currentSize), "Animal size must be finite and positive.");
            }

            LifetimeId = lifetimeId;
            SpeciesId = speciesId;
            Role = role;
            CurrentSize = currentSize;
        }

        public event Action Died;

        public long LifetimeId { get; }
        public string SpeciesId { get; }
        public TrophicRole Role { get; }
        public float CurrentSize { get; }
        public bool IsAlive { get; private set; } = true;
        public bool IsActive { get; private set; } = true;

        public void Retire()
        {
            IsActive = false;
        }

        public bool TryDie()
        {
            if (!IsAlive || !IsActive)
            {
                return false;
            }

            IsAlive = false;
            Died?.Invoke();
            return true;
        }
    }
}
