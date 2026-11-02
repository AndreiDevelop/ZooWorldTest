using NUnit.Framework;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Session;

namespace ZooWorld.Tests.EditMode
{
    public sealed class PredationServiceTests
    {
        private DeathStatisticsModel _statistics;
        private PredationService _service;
        private int _eatingEvents;

        [SetUp]
        public void SetUp()
        {
            _statistics = new DeathStatisticsModel();
            _service = new PredationService(_statistics);
            _eatingEvents = 0;
            _service.EatingCommitted += HandleEating;
        }

        [Test]
        public void PreyContactsOnlySeparateByPhysics()
        {
            var first = Animal(1, TrophicRole.Prey, 1f);
            var second = Animal(2, TrophicRole.Prey, 2f);
            Assert.That(_service.TryResolve(first, second, out var result), Is.False);
            Assert.That(result, Is.Null);
            Assert.That(first.IsAlive && second.IsAlive, Is.True);
            Assert.That(_eatingEvents, Is.Zero);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PredatorEatsLargerPreyOnceInEitherCallbackOrder(bool reverse)
        {
            var predator = Animal(1, TrophicRole.Predator, 0.5f);
            var prey = Animal(2, TrophicRole.Prey, 4f);
            var first = reverse ? prey : predator;
            var second = reverse ? predator : prey;
            Assert.That(_service.TryResolve(first, second, out var outcome), Is.True);
            Assert.That(outcome.Eater, Is.SameAs(predator));
            Assert.That(outcome.Victim, Is.SameAs(prey));
            Assert.That(_service.TryResolve(second, first, out _), Is.False);
            Assert.That(_service.TryResolve(first, second, out _), Is.False);
            Assert.That(_statistics.DeadPrey, Is.EqualTo(1));
            Assert.That(_statistics.DeadPredators, Is.Zero);
            Assert.That(_eatingEvents, Is.EqualTo(1));
            Assert.That(predator.CurrentSize, Is.EqualTo(0.5f));
        }

        [TestCase(1f, 2f, false, 2)]
        [TestCase(1f, 2f, true, 2)]
        [TestCase(2f, 1f, false, 1)]
        [TestCase(2f, 1f, true, 1)]
        [TestCase(1f, 1f, false, 1)]
        [TestCase(1f, 1f, true, 1)]
        public void PredatorWinnerUsesSizeThenLifetimeId(float firstSize, float secondSize, bool reverse, long winner)
        {
            var first = Animal(1, TrophicRole.Predator, firstSize);
            var second = Animal(2, TrophicRole.Predator, secondSize);
            Assert.That(_service.TryResolve(reverse ? second : first, reverse ? first : second, out var outcome), Is.True);
            Assert.That(outcome.EaterLifetimeId, Is.EqualTo(winner));
            Assert.That(outcome.Eater.IsAlive, Is.True);
            Assert.That(outcome.Victim.IsAlive, Is.False);
            Assert.That(_statistics.DeadPredators, Is.EqualTo(1));
            Assert.That(_eatingEvents, Is.EqualTo(1));
        }

        [Test]
        public void NullSelfAndDuplicateIdentityRequestsDoNotMutateState()
        {
            var predator = Animal(1, TrophicRole.Predator, 1f);
            var duplicate = Animal(1, TrophicRole.Prey, 1f);
            Assert.That(_service.TryResolve(null, predator, out _), Is.False);
            Assert.That(_service.TryResolve(predator, null, out _), Is.False);
            Assert.That(_service.TryResolve(predator, predator, out _), Is.False);
            Assert.That(_service.TryResolve(predator, duplicate, out _), Is.False);
            Assert.That(predator.IsAlive && duplicate.IsAlive, Is.True);
            Assert.That(_eatingEvents, Is.Zero);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DeadOrRetiredAnimalsCannotEatOrBeEaten(bool retire)
        {
            var predator = Animal(1, TrophicRole.Predator, 1f);
            var prey = Animal(2, TrophicRole.Prey, 1f);
            if (retire)
            {
                predator.Retire();
                prey.Retire();
            }
            else
            {
                predator.TryDie();
                prey.TryDie();
            }

            Assert.That(_service.TryResolve(predator, Animal(3, TrophicRole.Prey, 1f), out _), Is.False);
            Assert.That(_service.TryResolve(Animal(4, TrophicRole.Predator, 1f), prey, out _), Is.False);
            Assert.That(_statistics.DeadPrey + _statistics.DeadPredators, Is.Zero);
            Assert.That(_eatingEvents, Is.Zero);
        }

        [TestCase(TrophicRole.Prey)]
        [TestCase(TrophicRole.Predator)]
        [TestCase(TrophicRole.Omnivore)]
        public void UndefinedOmnivorePairsHaveNoImplicitOutcome(TrophicRole role)
        {
            var omnivore = Animal(1, TrophicRole.Omnivore, 2f);
            var other = Animal(2, role, 1f);
            Assert.That(_service.TryResolve(omnivore, other, out _), Is.False);
            Assert.That(_service.TryResolve(other, omnivore, out _), Is.False);
            Assert.That(omnivore.IsAlive && other.IsAlive, Is.True);
            Assert.That(_eatingEvents, Is.Zero);
        }

        [Test]
        public void MultiAnimalContactChainCommitsEachValidMealOnce()
        {
            var small = Animal(1, TrophicRole.Predator, 1f);
            var medium = Animal(2, TrophicRole.Predator, 2f);
            var large = Animal(3, TrophicRole.Predator, 3f);
            var prey = Animal(4, TrophicRole.Prey, 1f);
            Assert.That(_service.TryResolve(small, medium, out _), Is.True);
            Assert.That(_service.TryResolve(medium, large, out _), Is.True);
            Assert.That(_service.TryResolve(small, prey, out _), Is.False);
            Assert.That(_service.TryResolve(medium, prey, out _), Is.False);
            Assert.That(_service.TryResolve(large, prey, out _), Is.True);
            Assert.That(_statistics.DeadPredators, Is.EqualTo(2));
            Assert.That(_statistics.DeadPrey, Is.EqualTo(1));
            Assert.That(_eatingEvents, Is.EqualTo(3));
        }

        [Test]
        public void EarlierDeathPreventsLaterContactFromEatingAnotherAnimal()
        {
            var large = Animal(1, TrophicRole.Predator, 3f);
            var medium = Animal(2, TrophicRole.Predator, 2f);
            var prey = Animal(3, TrophicRole.Prey, 1f);
            _service.TryResolve(large, medium, out _);
            Assert.That(_service.TryResolve(medium, prey, out _), Is.False);
            Assert.That(prey.IsAlive, Is.True);
            Assert.That(_statistics.DeadPrey, Is.Zero);
        }

        [Test]
        public void StateAndCounterCommitBeforeObserversAndReentrantCallbacksCannotRepeatDeath()
        {
            var predator = Animal(1, TrophicRole.Predator, 1f);
            var prey = Animal(2, TrophicRole.Prey, 1f);
            var diedEvents = 0;
            var counterEvents = 0;
            prey.Died += () =>
            {
                diedEvents++;
                Assert.That(prey.IsAlive, Is.False);
                Assert.That(_statistics.DeadPrey, Is.EqualTo(1));
                Assert.That(_service.TryResolve(prey, predator, out _), Is.False);
            };
            _statistics.Changed += () =>
            {
                counterEvents++;
                Assert.That(_service.TryResolve(predator, prey, out _), Is.False);
            };
            Assert.That(_service.TryResolve(predator, prey, out _), Is.True);
            Assert.That(diedEvents, Is.EqualTo(1));
            Assert.That(counterEvents, Is.EqualTo(1));
            Assert.That(_eatingEvents, Is.EqualTo(1));
        }

        [Test]
        public void RecordedIdentityCannotBeReintroducedAsAnotherLiveVictim()
        {
            var predator = Animal(1, TrophicRole.Predator, 1f);
            _service.TryResolve(predator, Animal(2, TrophicRole.Prey, 1f), out _);
            var reusedIdentity = Animal(2, TrophicRole.Prey, 1f);
            Assert.That(_service.TryResolve(predator, reusedIdentity, out _), Is.False);
            Assert.That(reusedIdentity.IsAlive, Is.True);
            Assert.That(_statistics.DeadPrey, Is.EqualTo(1));
            Assert.That(_eatingEvents, Is.EqualTo(1));
        }

        private void HandleEating(EatingEvent outcome)
        {
            Assert.That(outcome.Victim.IsAlive, Is.False);
            _eatingEvents++;
        }

        private static AnimalModel Animal(long id, TrophicRole role, float size)
        {
            return new AnimalModel(id, role.ToString(), role, size);
        }
    }
}
