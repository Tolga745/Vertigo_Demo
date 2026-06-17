using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using VertigoWheel.Core;
using VertigoWheel.Domain;

namespace VertigoWheel.UI
{
    /// <summary>
    /// Horizontal progress bar that shows the current zone and the zones coming up. Normal zones are
    /// neutral, safe zones green, super zones gold, and the current zone is enlarged with a highlight box
    /// — mirroring the Critical Strike reference. When the player advances by exactly one zone the whole
    /// strip slides forward one cell and the new current zone pops in.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ZoneBarView : MonoBehaviour
    {
        [SerializeField] private RectTransform _cellsContainer; // zone_cells (the part that slides)
        [SerializeField] private ZoneCellView[] _cells = System.Array.Empty<ZoneCellView>();

        [Header("Colors")]
        [SerializeField] private Color _normalColor = new Color(0.95f, 0.95f, 0.92f);
        [SerializeField] private Color _safeColor = new Color(0.49f, 0.99f, 0.24f);
        [SerializeField] private Color _superColor = new Color(1f, 0.76f, 0.24f);
        [SerializeField] private Color _currentNumberColor = new Color(0.10f, 0.12f, 0.08f);
        [SerializeField] private Color _currentHighlightColor = new Color(0.49f, 0.99f, 0.24f);

        [Header("Animation")]
        [SerializeField] private float _currentScale = 1.14f;
        [SerializeField] private float _slideDistance = 140f;
        [SerializeField] private float _slideDuration = 0.35f;

        private int _lastCurrentIndex = int.MinValue;

        public void SetZones(IReadOnlyList<ZoneCellData> zones)
        {
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] == null) continue;

                if (i >= zones.Count)
                {
                    _cells[i].SetContent(0, _normalColor, false, _currentHighlightColor, false);
                    continue;
                }

                var z = zones[i];
                Color numberColor = z.IsCurrent ? _currentNumberColor : ColorFor(z.Type);
                _cells[i].SetContent(z.Index, numberColor, z.IsCurrent, _currentHighlightColor, true);
                _cells[i].transform.DOKill();
                _cells[i].transform.localScale = Vector3.one * (z.IsCurrent ? _currentScale : 1f);
            }

            int newCurrent = zones.Count > 0 ? zones[0].Index : 0;
            bool steppedForward = newCurrent == _lastCurrentIndex + 1;
            _lastCurrentIndex = newCurrent;

            if (steppedForward) PlayAdvanceAnimation();
        }

        private void PlayAdvanceAnimation()
        {
            // Slide the whole strip forward one cell.
            if (_cellsContainer != null)
            {
                float baseY = _cellsContainer.anchoredPosition.y;
                _cellsContainer.DOKill();
                DOTween.To(
                    () => _slideDistance,
                    x => _cellsContainer.anchoredPosition = new Vector2(x, baseY),
                    0f, _slideDuration).SetEase(Ease.OutCubic);
            }

            // Pop the new current cell in.
            if (_cells.Length > 0 && _cells[0] != null)
            {
                var t = _cells[0].transform;
                t.DOKill();
                t.localScale = Vector3.one * 0.85f;
                t.DOScale(Vector3.one * _currentScale, 0.35f).SetEase(Ease.OutBack);
            }
        }

        private Color ColorFor(ZoneType type) => type switch
        {
            ZoneType.Safe => _safeColor,
            ZoneType.Super => _superColor,
            _ => _normalColor
        };

        private void OnValidate()
        {
            if (_cellsContainer == null)
            {
                var found = UiBinder.FindByName<RectTransform>(transform, "zone_cells");
                if (found != null) _cellsContainer = found;
            }
            if (_cells == null || _cells.Length == 0)
                _cells = GetComponentsInChildren<ZoneCellView>(true);
        }
    }
}
