using System;
using NUnit.Framework;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Session;

namespace ZooWorld.Tests.EditMode
{
    public sealed class SessionModelTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void SessionRequiresBothSceneReadinessSignals(bool mainFirst)
        {
            var session = new SessionModel();
            Assert.That(session.CanSimulate, Is.False);
            session.BeginLoading();
            if (mainFirst)
            {
                session.MarkMainReady();
            }
            else
            {
                session.MarkHudReady();
            }

            Assert.That(session.CanSimulate, Is.False);
            if (mainFirst)
            {
                session.MarkHudReady();
            }
            else
            {
                session.MarkMainReady();
            }

            Assert.That(session.CanSimulate, Is.True);
            session.BeginStopping();
            Assert.That(session.CanSimulate, Is.False);
            Assert.That(session.MainReady || session.HudReady, Is.False);
            session.MarkStopped();
            Assert.Throws<InvalidOperationException>(() => session.MarkMainReady());
        }

        [Test]
        public void LoadingCannotRestartAnExistingSession()
        {
            var session = new SessionModel();
            session.BeginLoading();
            Assert.Throws<InvalidOperationException>(() => session.BeginLoading());
            session.Fail("Missing camera");
            Assert.That(session.CanSimulate, Is.False);
            Assert.That(session.FailureReason, Is.EqualTo("Missing camera"));
        }

        [Test]
        public void DeathIsCommittedAndCountedOncePerLifetime()
        {
            var animal = new AnimalModel(1, "frog", TrophicRole.Prey, 1f);
            var statistics = new DeathStatisticsModel();
            var changes = 0;
            statistics.Changed += () => changes++;
            Assert.That(statistics.RecordDeath(animal), Is.False);
            Assert.That(animal.TryDie(), Is.True);
            Assert.That(statistics.RecordDeath(animal), Is.True);
            Assert.That(animal.TryDie(), Is.False);
            Assert.That(statistics.RecordDeath(animal), Is.False);
            Assert.That(statistics.DeadPrey, Is.EqualTo(1));
            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void OmnivoreIsNeverSilentlyMappedToAnExistingCounter()
        {
            var animal = new AnimalModel(1, "future", TrophicRole.Omnivore, 1f);
            animal.TryDie();
            var statistics = new DeathStatisticsModel();
            Assert.Throws<InvalidOperationException>(() => statistics.RecordDeath(animal));
            Assert.That(statistics.DeadPrey + statistics.DeadPredators, Is.Zero);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void InvalidInstanceSizesAreRejected(float size)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AnimalModel(1, "frog", TrophicRole.Prey, size));
        }

        [Test]
        public void LifetimeSequenceNeverReusesAnIdentity()
        {
            var sequence = new AnimalIdentitySequence();
            var first = sequence.Next();
            var second = sequence.Next();
            Assert.That(second, Is.GreaterThan(first));
        }
    }
}
