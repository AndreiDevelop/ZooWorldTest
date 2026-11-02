using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Zenject;
using ZooWorld.Animals;
using ZooWorld.Animals.Content;
using ZooWorld.Domain.Animals;
using ZooWorld.Domain.Session;
using ZooWorld.Infrastructure;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ZooWorld.Tests.EditMode
{
    public sealed class AnimalConfigurationTests
    {
        private AnimalDefinitionSO _definition;
        private TestMovementSO _movement;
        private GameObject _object;
        private AnimalView _view;
        private Texture2D _texture;
        private Sprite _icon;

        [SetUp]
        public void SetUp()
        {
            _definition = ScriptableObject.CreateInstance<AnimalDefinitionSO>();
            _movement = ScriptableObject.CreateInstance<TestMovementSO>();
            _object = new GameObject("TestAnimal");
            _object.SetActive(false);
            var body = _object.AddComponent<Rigidbody>();
            _view = _object.AddComponent<AnimalView>();
            var visual = new GameObject("Visual");
            visual.transform.SetParent(_object.transform);
            var viewData = new SerializedObject(_view);
            viewData.FindProperty("_body").objectReferenceValue = body;
            viewData.FindProperty("_visual").objectReferenceValue = visual.transform;
            viewData.ApplyModifiedPropertiesWithoutUndo();
            _texture = new Texture2D(2, 2);
            _icon = Sprite.Create(_texture, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
            var data = new SerializedObject(_definition);
            data.FindProperty("_id").stringValue = "frog";
            data.FindProperty("_displayName").stringValue = "Frog";
            data.FindProperty("_icon").objectReferenceValue = _icon;
            data.FindProperty("_movement").objectReferenceValue = _movement;
            data.FindProperty("_prefab").objectReferenceValue = _view;
            data.FindProperty("_sizeRange").vector2Value = new Vector2(0.5f, 1.5f);
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_object);
            Object.DestroyImmediate(_definition);
            Object.DestroyImmediate(_movement);
            Object.DestroyImmediate(_icon);
            Object.DestroyImmediate(_texture);
        }

        [TestCase(0f, 1f)]
        [TestCase(2f, 1f)]
        [TestCase(float.NaN, 1f)]
        [TestCase(1f, float.PositiveInfinity)]
        public void InvalidAuthoredSizeRangeFailsBeforeCreation(float minimum, float maximum)
        {
            SetRange(new Vector2(minimum, maximum));
            Assert.Throws<InvalidOperationException>(() => _definition.Validate());
        }

        [TestCase("_icon")]
        [TestCase("_movement")]
        [TestCase("_prefab")]
        public void MissingContentReferenceHasAnActionableError(string field)
        {
            var data = new SerializedObject(_definition);
            data.FindProperty(field).objectReferenceValue = null;
            data.ApplyModifiedPropertiesWithoutUndo();
            var error = Assert.Throws<InvalidOperationException>(() => _definition.Validate());
            Assert.That(error.Message, Does.Contain("frog"));
        }

        [Test]
        public void OmnivoreCanBeAuthoredButCannotEnterSimulation()
        {
            var data = new SerializedObject(_definition);
            data.FindProperty("_role").enumValueIndex = (int)TrophicRole.Omnivore;
            data.ApplyModifiedPropertiesWithoutUndo();
            Assert.DoesNotThrow(() => _definition.Validate());
            Assert.Throws<InvalidOperationException>(() => _definition.ValidateForSimulation());
        }

        [Test]
        public void SamplingStaysInRangeAndDoesNotMutateSharedConfiguration()
        {
            var before = EditorJsonUtility.ToJson(_definition);
            var randomState = Random.state;
            try
            {
                for (var index = 0; index < 100; index++)
                {
                    Assert.That(_definition.SampleSize(), Is.InRange(0.5f, 1.5f));
                }
            }
            finally
            {
                Random.state = randomState;
            }

            Assert.That(EditorJsonUtility.ToJson(_definition), Is.EqualTo(before));
        }

        [Test]
        public void FactoryInjectsOneModelIntoControllerAndViewAndReplacesOldLifetime()
        {
            SetRange(Vector2.one);
            var container = new DiContainer();
            SessionInstaller.Install(container);
            container.Bind<AnimalLifetimeFactory>().AsSingle();
            var factory = container.Resolve<AnimalLifetimeFactory>();
            var first = factory.Initialize(_definition, _view);
            Assert.That(_view.Model, Is.SameAs(first.Model));
            Assert.That(first.CanSimulate, Is.False);
            var second = factory.Initialize(_definition, _view);
            Assert.That(second.Model.LifetimeId, Is.GreaterThan(first.Model.LifetimeId));
            Assert.That(_view.Model, Is.SameAs(second.Model));
            first.Model.TryDie();
            Assert.That(_view.Visual.gameObject.activeSelf, Is.True);
            var session = container.Resolve<SessionModel>();
            session.BeginLoading();
            session.MarkMainReady();
            session.MarkHudReady();
            Assert.That(second.CanSimulate, Is.True);
            session.BeginStopping();
            Assert.That(second.CanSimulate, Is.False);
        }

        private void SetRange(Vector2 range)
        {
            var data = new SerializedObject(_definition);
            data.FindProperty("_sizeRange").vector2Value = range;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
