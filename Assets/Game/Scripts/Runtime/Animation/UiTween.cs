using System;
using DG.Tweening;
using UnityEngine;

namespace VertigoWheel.Animation
{
    /// <summary>
    /// Thin wrapper around DOTween for the handful of UI motions the game needs. Centralising the
    /// tween calls here keeps the DOTween dependency in one place and gives views a tidy API.
    /// Uses only core DOTween shortcuts (transform/float), so no extra UI module is required.
    /// </summary>
    public static class UiTween
    {
        public static Tween FadeCanvasGroup(CanvasGroup group, float target, float duration,
            Action onComplete = null)
        {
            group.DOKill();
            return DOTween.To(() => group.alpha, a => group.alpha = a, target, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => onComplete?.Invoke())
                .SetUpdate(true); // independent of Time.timeScale
        }

        /// <summary>Scale-in pop used when a popup appears.</summary>
        public static Tween PopIn(Transform t, float duration = 0.35f)
        {
            t.DOKill();
            t.localScale = Vector3.one * 0.7f;
            return t.DOScale(Vector3.one, duration).SetEase(Ease.OutBack).SetUpdate(true);
        }

        /// <summary>Quick squash-and-stretch feedback for button presses / reward grants.</summary>
        public static Tween Punch(Transform t, float strength = 0.18f, float duration = 0.3f)
        {
            t.DOKill();
            t.localScale = Vector3.one;
            return t.DOPunchScale(Vector3.one * strength, duration, 6, 0.7f).SetUpdate(true);
        }
    }
}
