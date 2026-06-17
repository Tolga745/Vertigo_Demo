using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VertigoWheel.Config;
using VertigoWheel.Core;

namespace VertigoWheel.UI
{
    /// <summary>Art for one wheel tier (swapped at runtime as the player moves between zone types).</summary>
    [Serializable]
    public struct WheelTierVisual
    {
        public WheelTier tier;
        public Sprite baseSprite;
        public Sprite indicatorSprite;
    }

    /// <summary>
    /// Visual + animation for the revolver wheel. Lays its 8 chambers out in a ring (in the editor too,
    /// so the builder produces a correct hierarchy), swaps tier art, and spins to a chosen chamber with
    /// DOTween. Child references are auto-bound in <see cref="OnValidate"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WheelView : MonoBehaviour
    {
        [Header("Auto-bound references")]
        [SerializeField] private Image _baseImage;      // ui_image_wheel_base_value
        [SerializeField] private Image _indicatorImage; // ui_image_wheel_indicator_value
        [SerializeField] private RectTransform _slicesRoot; // wheel_slices (this is what rotates)
        [SerializeField] private SliceView[] _slices = Array.Empty<SliceView>();

        [Header("Tier art")]
        [SerializeField] private List<WheelTierVisual> _tierVisuals = new List<WheelTierVisual>();

        [Header("Chamber layout (ratios of the wheel size)")]
        [Range(0.1f, 0.45f)] [SerializeField] private float _sliceRadiusRatio = 0.305f;
        [Range(0.1f, 0.4f)] [SerializeField] private float _sliceSizeRatio = 0.165f;
        [Tooltip("Rotate each chamber so its label points outward (radial). Off = items stay upright.")]
        [SerializeField] private bool _radialSlices = false;

        [Header("Spin animation")]
        [SerializeField] private float _spinDuration = 3.2f;
        [SerializeField] private int _spinExtraTurns = 5;
        [SerializeField] private Ease _spinEase = Ease.OutQuart;

        private float _currentZ;

        public int SliceCount => _slices.Length;

        /// <summary>Swaps tier art and fills every chamber with pre-resolved display content.</summary>
        public void Configure(WheelTier tier, IReadOnlyList<SliceDisplay> slices)
        {
            ApplyTierVisual(tier);
            ResetRotation();

            int n = Mathf.Min(_slices.Length, slices.Count);
            for (int i = 0; i < _slices.Length; i++)
            {
                if (i < n)
                {
                    var d = slices[i];
                    _slices[i].SetContent(d.Icon, d.Text, d.IsBomb);
                    _slices[i].gameObject.SetActive(true);
                }
                else
                {
                    _slices[i].gameObject.SetActive(false);
                }
            }
        }

        /// <summary>Spins the cylinder so chamber <paramref name="index"/> stops under the top indicator.</summary>
        public void SpinTo(int index, Action onComplete)
        {
            if (_slicesRoot == null) { onComplete?.Invoke(); return; }

            float anglePerSlice = 360f / Mathf.Max(1, _slices.Length);
            float targetMod = index * anglePerSlice;                 // chamber's resting angle
            float currentMod = Mathf.Repeat(_currentZ, 360f);
            // Spin clockwise: decrease the angle until we reach the target slice.
            float backward = Mathf.Repeat(currentMod - targetMod, 360f);
            float final = _currentZ - backward - 360f * _spinExtraTurns;

            _slicesRoot.DOKill();
            DOTween.To(() => _currentZ, ApplyRotation, final, _spinDuration)
                .SetEase(_spinEase)
                .OnComplete(() => onComplete?.Invoke());
        }

        /// <summary>
        /// Rotates the cylinder (base + chambers) to <paramref name="z"/> while counter-rotating each
        /// item so the icons orbit with the wheel but always stay upright (never flipped at the bottom).
        /// </summary>
        private void ApplyRotation(float z)
        {
            _currentZ = z;
            if (_slicesRoot != null) _slicesRoot.localEulerAngles = new Vector3(0f, 0f, z);

            var upright = new Vector3(0f, 0f, -z);
            for (int i = 0; i < _slices.Length; i++)
                if (_slices[i] != null) _slices[i].transform.localEulerAngles = upright;
        }

        public void ResetRotation()
        {
            if (_slicesRoot != null) _slicesRoot.DOKill();
            ApplyRotation(0f);
        }

        private void ApplyTierVisual(WheelTier tier)
        {
            foreach (var v in _tierVisuals)
            {
                if (v.tier != tier) continue;
                if (_baseImage != null && v.baseSprite != null) _baseImage.sprite = v.baseSprite;
                if (_indicatorImage != null && v.indicatorSprite != null) _indicatorImage.sprite = v.indicatorSprite;
                return;
            }
        }

        /// <summary>
        /// Positions chambers evenly around the ring. Public so the editor builder can lay them out at
        /// authoring time, producing a correct, inspectable hierarchy.
        /// </summary>
        public void LayoutSlices()
        {
            if (_slicesRoot == null || _slices == null) return;

            var rootRect = _slicesRoot.rect;
            float wheelSize = Mathf.Min(rootRect.width, rootRect.height);
            if (wheelSize <= 0f && _baseImage != null)
                wheelSize = Mathf.Min(_baseImage.rectTransform.rect.width, _baseImage.rectTransform.rect.height);

            float radius = wheelSize * _sliceRadiusRatio;
            float size = wheelSize * _sliceSizeRatio;
            float anglePerSlice = 360f / Mathf.Max(1, _slices.Length);

            for (int i = 0; i < _slices.Length; i++)
            {
                if (_slices[i] == null) continue;
                var rt = (RectTransform)_slices[i].transform;
                float deg = i * anglePerSlice;                 // clockwise from top
                float rad = deg * Mathf.Deg2Rad;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(Mathf.Sin(rad) * radius, Mathf.Cos(rad) * radius);
                rt.sizeDelta = new Vector2(size, size);
                rt.localEulerAngles = _radialSlices ? new Vector3(0f, 0f, -deg) : Vector3.zero;
            }
        }

        private void OnValidate()
        {
            if (_baseImage == null) _baseImage = UiBinder.FindByName<Image>(transform, "ui_image_wheel_base_value");
            if (_indicatorImage == null) _indicatorImage = UiBinder.FindByName<Image>(transform, "ui_image_wheel_indicator_value");
            if (_slicesRoot == null)
            {
                var found = UiBinder.FindByName<RectTransform>(transform, "wheel_slices");
                if (found != null) _slicesRoot = found;
            }
            if ((_slices == null || _slices.Length == 0) && _slicesRoot != null)
                _slices = _slicesRoot.GetComponentsInChildren<SliceView>(true);
        }
    }
}
