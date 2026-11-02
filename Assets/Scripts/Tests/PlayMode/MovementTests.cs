using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using ZooWorld.Animals.Content;
using Object = UnityEngine.Object;

namespace ZooWorld.Tests.PlayMode
{
    public sealed class MovementTests
    {
        private readonly List<Object> _settings = new List<Object>();

        private Scene _scene;
        private PhysicsScene _physics;

        [SetUp]
        public void SetUp()
        {
            _scene = SceneManager.CreateScene(
                "MovementTest",
                new CreateSceneParameters(LocalPhysicsMode.Physics3D));
            _physics = _scene.GetPhysicsScene();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var settings in _settings)
            {
                Object.Destroy(settings);
            }

            _settings.Clear();

            if (_scene.IsValid() && _scene.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(_scene);
            }
        }

        [TestCase(0.02f)]
        [TestCase(0.05f)]
        public void LinearMovementTravelsAtConfiguredSpeed(float step)
        {
            var settings = CreateSettings<LinearMovementSO>();
            SetField(settings, "_speed", 3f);
            SetField(settings, "_recoveryAcceleration", 4f);
            var body = CreateBody(Vector3.zero);
            var movement = settings.Create(body, body.transform);
            var steps = Mathf.RoundToInt(1f / step);

            for (var i = 0; i < steps; i++)
            {
                movement.Tick(new Vector3(5f, 2f, 0f), step);
                _physics.Simulate(step);
            }

            Assert.That(body.position.x, Is.EqualTo(3f).Within(0.01f));
            Assert.That(body.position.y, Is.EqualTo(0f).Within(0.001f));
            Assert.That(body.position.z, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void JumpTravelsFixedDistanceAndReturnsVisualToRest()
        {
            const float Step = 0.02f;
            var settings = CreateJumpSettings();
            var body = CreateBody(Vector3.zero);
            var visual = CreateVisual(body.transform);
            var restPosition = visual.localPosition;
            var movement = settings.Create(body, visual);
            var highestVisualPosition = restPosition.y;
            var steps = Mathf.CeilToInt(settings.Duration / Step);

            for (var i = 0; i < steps; i++)
            {
                movement.Tick(Vector3.right, Step);
                _physics.Simulate(Step);
                highestVisualPosition = Mathf.Max(highestVisualPosition, visual.localPosition.y);
            }

            movement.Tick(Vector3.right, Step);
            _physics.Simulate(Step);

            Assert.That(body.position.x, Is.EqualTo(settings.Distance).Within(0.01f));
            Assert.That(body.position.y, Is.EqualTo(0f).Within(0.001f));
            Assert.That(highestVisualPosition, Is.GreaterThan(restPosition.y + settings.Height * 0.9f));
            Assert.That(Vector3.Distance(visual.localPosition, restPosition), Is.LessThan(0.001f));
            Assert.That(body.linearVelocity.sqrMagnitude, Is.LessThan(0.001f));
        }

        [Test]
        public void MotorRetainsPhysicalRecoilWhileRecoveringCommandedVelocity()
        {
            const float Step = 0.02f;
            var settings = CreateSettings<LinearMovementSO>();
            SetField(settings, "_speed", 2f);
            SetField(settings, "_recoveryAcceleration", 4f);
            var body = CreateBody(Vector3.zero);
            var movement = settings.Create(body, body.transform);

            movement.Tick(Vector3.right, Step);
            body.AddForce(Vector3.forward * 5f, ForceMode.VelocityChange);
            _physics.Simulate(Step);
            movement.Tick(Vector3.right, Step);

            Assert.That(body.linearVelocity.x, Is.EqualTo(settings.Speed).Within(0.001f));
            Assert.That(body.linearVelocity.z, Is.GreaterThan(4f));
            Assert.That(body.linearVelocity.z, Is.LessThan(5f));
        }

        [Test]
        public void SharedJumpSettingsCreateIndependentMotorsWithoutChangingAsset()
        {
            var settings = CreateJumpSettings();
            var initialSettings = JsonUtility.ToJson(settings);
            var firstBody = CreateBody(Vector3.left * 5f);
            var secondBody = CreateBody(Vector3.right * 5f);
            var firstVisual = CreateVisual(firstBody.transform);
            var secondVisual = CreateVisual(secondBody.transform);
            var first = settings.Create(firstBody, firstVisual);
            var second = settings.Create(secondBody, secondVisual);

            first.Tick(Vector3.right, 0.2f);
            second.Tick(Vector3.forward, 0.1f);
            var secondPosition = secondVisual.localPosition;
            first.Tick(Vector3.right, 0.2f);

            Assert.That(Vector3.Distance(secondVisual.localPosition, secondPosition), Is.LessThan(0.001f));
            Assert.That(firstBody.linearVelocity.x, Is.GreaterThan(0f));
            Assert.That(firstBody.linearVelocity.z, Is.EqualTo(0f).Within(0.001f));
            Assert.That(secondBody.linearVelocity.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(secondBody.linearVelocity.z, Is.GreaterThan(0f));
            Assert.That(JsonUtility.ToJson(settings), Is.EqualTo(initialSettings));
        }

        [Test]
        public void CollidingAnimalsReboundAndTheMotorKeepsTheirPhysicalSeparation()
        {
            const float Step = 0.02f;
            var material = new PhysicsMaterial("TestRebound");
            material.bounciness = 0.8f;
            material.dynamicFriction = 0f;
            material.staticFriction = 0f;
            material.bounceCombine = PhysicsMaterialCombine.Maximum;
            _settings.Add(material);
            var first = CreateBody(Vector3.left * 0.51f);
            var second = CreateBody(Vector3.right * 0.51f);
            first.gameObject.AddComponent<SphereCollider>().sharedMaterial = material;
            second.gameObject.AddComponent<SphereCollider>().sharedMaterial = material;
            var settings = CreateSettings<LinearMovementSO>();
            SetField(settings, "_speed", 2f);
            SetField(settings, "_recoveryAcceleration", 4f);
            var firstMovement = settings.Create(first, first.transform);
            var secondMovement = settings.Create(second, second.transform);
            firstMovement.Tick(Vector3.right, Step);
            secondMovement.Tick(Vector3.left, Step);
            _physics.Simulate(Step);
            _physics.Simulate(Step);
            firstMovement.Tick(Vector3.right, Step);
            secondMovement.Tick(Vector3.left, Step);
            Assert.That(first.linearVelocity.x, Is.LessThan(0f));
            Assert.That(second.linearVelocity.x, Is.GreaterThan(0f));
        }

        [Test]
        public void InvalidAndNonfiniteSettingsAreRejectedBeforeMotorCreation()
        {
            var body = CreateBody(Vector3.zero);
            var visual = CreateVisual(body.transform);
            var jump = CreateJumpSettings();
            SetField(jump, "_duration", 0f);

            Assert.That(jump.IsValid, Is.False);
            Assert.Throws<InvalidOperationException>(() => jump.Create(body, visual));

            SetField(jump, "_duration", 0.45f);
            SetField(jump, "_distance", float.PositiveInfinity);
            Assert.That(jump.IsValid, Is.False);

            var linear = CreateSettings<LinearMovementSO>();
            SetField(linear, "_speed", float.NaN);
            Assert.That(linear.IsValid, Is.False);
            Assert.Throws<InvalidOperationException>(() => linear.Create(body, visual));

            SetField(linear, "_speed", 2f);
            SetField(linear, "_recoveryAcceleration", 0f);
            Assert.That(linear.IsValid, Is.False);
        }

        private T CreateSettings<T>() where T : ScriptableObject
        {
            var settings = ScriptableObject.CreateInstance<T>();
            _settings.Add(settings);
            return settings;
        }

        private JumpMovementSO CreateJumpSettings()
        {
            var settings = CreateSettings<JumpMovementSO>();
            SetField(settings, "_interval", 1.2f);
            SetField(settings, "_duration", 0.45f);
            SetField(settings, "_distance", 1.8f);
            SetField(settings, "_height", 0.7f);
            SetField(settings, "_recoveryAcceleration", 4f);
            return settings;
        }

        private Rigidbody CreateBody(Vector3 position)
        {
            var gameObject = new GameObject("TestAnimal");
            SceneManager.MoveGameObjectToScene(gameObject, _scene);
            gameObject.transform.position = position;
            var body = gameObject.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.linearDamping = 0f;
            body.angularDamping = 0f;
            body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            return body;
        }

        private static Transform CreateVisual(Transform parent)
        {
            var visual = new GameObject("Visual").transform;
            visual.SetParent(parent, false);
            visual.localPosition = Vector3.up * 0.3f;
            return visual;
        }

        private static void SetField(ScriptableObject settings, string fieldName, float value)
        {
            var field = settings.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing authored setting '{fieldName}'.");
            field.SetValue(settings, value);
        }
    }
}
