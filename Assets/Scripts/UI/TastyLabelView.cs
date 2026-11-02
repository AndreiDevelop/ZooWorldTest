using System;
using TMPro;
using UnityEngine;

namespace ZooWorld.UI
{
    public sealed class TastyLabelView : MonoBehaviour
    {
        [SerializeField] private RectTransform _rect;
        [SerializeField] private TMP_Text _text;
        [SerializeField] private CanvasGroup _group;

        public Vector2 Position => _rect.anchoredPosition;

        public void ValidateReferences()
        {
            if (_rect == null || _text == null || _group == null)
            {
                throw new InvalidOperationException("Tasty label requires a RectTransform, TMP text and CanvasGroup.");
            }
        }

        public void Show(Vector2 position, float opacity)
        {
            _rect.anchoredPosition = position;
            _group.alpha = opacity;
        }

        public void SetFontSize(float size)
        {
            _text.fontSize = size;
        }

        public void ResetPresentation()
        {
            _rect.anchoredPosition = Vector2.zero;
            _group.alpha = 1f;
        }
    }
}
