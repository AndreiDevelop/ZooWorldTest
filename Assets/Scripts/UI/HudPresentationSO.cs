using System;
using UnityEngine;

namespace ZooWorld.UI
{
    [CreateAssetMenu(menuName = "ZooWorld/HUD Presentation")]
    public sealed class HudPresentationSO : ScriptableObject
    {
        [SerializeField] private float _counterFontSize = 22f;
        [SerializeField] private float _feedbackFontSize = 24f;
        [SerializeField] private float _feedbackDuration = 1f;
        [SerializeField] private Vector2 _feedbackOffset = new Vector2(0f, -18f);
        [SerializeField] private float _stackSpacing = 25f;
        [SerializeField] private float _riseDistance = 10f;
        [SerializeField] private int _maxInactiveLabels = 16;

        public float CounterFontSize => _counterFontSize;
        public float FeedbackFontSize => _feedbackFontSize;
        public float FeedbackDuration => _feedbackDuration;
        public Vector2 FeedbackOffset => _feedbackOffset;
        public float StackSpacing => _stackSpacing;
        public float RiseDistance => _riseDistance;
        public int MaxInactiveLabels => _maxInactiveLabels;

        public void Validate()
        {
            if (!float.IsFinite(_counterFontSize) || _counterFontSize <= 0f
                || !float.IsFinite(_feedbackFontSize) || _feedbackFontSize <= 0f
                || !float.IsFinite(_feedbackDuration) || _feedbackDuration <= 0f
                || !float.IsFinite(_feedbackOffset.x) || !float.IsFinite(_feedbackOffset.y)
                || !float.IsFinite(_stackSpacing) || _stackSpacing <= 0f
                || !float.IsFinite(_riseDistance) || _riseDistance < 0f || _maxInactiveLabels < 0)
            {
                throw new InvalidOperationException("HUD presentation requires finite offsets, positive duration/spacing and nonnegative retention.");
            }
        }
    }
}
