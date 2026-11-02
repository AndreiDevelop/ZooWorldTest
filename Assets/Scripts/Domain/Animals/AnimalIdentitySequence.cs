using System;

namespace ZooWorld.Domain.Animals
{
    public sealed class AnimalIdentitySequence
    {
        private long _lastId;

        public long Next()
        {
            if (_lastId == long.MaxValue)
            {
                throw new InvalidOperationException("Animal lifetime identities are exhausted.");
            }

            _lastId++;
            return _lastId;
        }
    }
}
