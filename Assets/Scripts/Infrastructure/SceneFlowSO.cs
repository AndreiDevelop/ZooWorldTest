using System;
using UnityEngine;

namespace ZooWorld.Infrastructure
{
    [CreateAssetMenu(menuName = "ZooWorld/Scene Flow")]
    public sealed class SceneFlowSO : ScriptableObject
    {
        [SerializeField] private string _mainScenePath;
        [SerializeField] private string _hudScenePath;

        public string MainScenePath => _mainScenePath;
        public string HudScenePath => _hudScenePath;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(_mainScenePath) || string.IsNullOrWhiteSpace(_hudScenePath)
                || !_mainScenePath.EndsWith(".unity", StringComparison.Ordinal)
                || !_hudScenePath.EndsWith(".unity", StringComparison.Ordinal)
                || _mainScenePath == _hudScenePath)
            {
                throw new InvalidOperationException("Scene flow requires distinct Main and HUD scene asset paths.");
            }
        }
    }
}
